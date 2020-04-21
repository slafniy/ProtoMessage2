using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProtoMessageOriginal
{

    public struct AttrMatrixElement
    {
        private readonly int _index; // global "position" in text, '{' for message and ':' for attribute
        public readonly int? Level; // increases on each '{' decreases on each '}'
        public string Value => _value ?? ParseAttributeValue();
        private string? _value;
        private readonly string _protoAsText;

        public AttrMatrixElement(int index, int? level, string protoAsText)
        {
            _index = index;
            Level = level;
            _value = null;
            _protoAsText = protoAsText;
        }

        // For debug purposes
        public override string ToString()
        {
            return $"Index: {_index} Level: {Level} ";
        }

        // Do not save the result! Substring is too slow!
        public bool CheckName(string name)
        {
            int nameIdx = name.Length - 1;  // the end of the name
            int attrIdx = _index - 1;  // skip colon

            while (nameIdx >= 0)
            {
                if (name[nameIdx] != _protoAsText[attrIdx])
                {
                    return false;
                }
                attrIdx--;
                nameIdx--;
            }

            attrIdx--;
            bool res = attrIdx < 0 || _protoAsText[attrIdx] == ' ' || _protoAsText[attrIdx] == '\n';
            return res;
        }

        private string ParseAttributeValue()
        {
            int index = _index;
            int start = index + 2; // skip whitespace
            while (index < _protoAsText.Length && _protoAsText[index] != '\n')
            {
                index++;
            }

            _value = _protoAsText.Substring(start, index - start).Trim('"');
            return _value;
        }
    }
    
    
    public struct MsgMatrixElement
    {
        private readonly int _index; // global "position" in text, '{' for message and ':' for attribute
        public readonly int? Level; // increases on each '{' decreases on each '}'
        public readonly List<int> ChildIndexes; // sub-messages in matrix
        public readonly List<int> AttributeIndexes; // attribute index in matrix
        public string? Name => _name ?? ParseName();
        private string? _name;
        private readonly string _protoAsText;

        public MsgMatrixElement(int index, int? level, string protoAsText)
        {
            _index = index;
            Level = level;
            ChildIndexes = new List<int>();
            AttributeIndexes = new List<int>();
            _name = null;
            _protoAsText = protoAsText;
        }

        // For debug purposes
        public override string ToString()
        {
            return $"Index: {_index} Level: {Level} " +
                   $"ChildIndexes: {string.Join(", ", ChildIndexes)} " +
                   $"AttributeIndexes: {string.Join(", ", AttributeIndexes)}";
        }

        public bool CheckName(string name)
        {
            int nameIdx = name.Length - 1;  // the end of the name
            int msgIdx = _index - 2;  // skip '{' and whitespace
            
            // Look backward for newline or message beginning
            while (nameIdx >= 0)
            {
                if (name[nameIdx] != _protoAsText[msgIdx])
                {
                    return false;
                }

                msgIdx--;
                nameIdx--;
            }
            
            bool res = msgIdx < 0 || _protoAsText[msgIdx] == ' ' || _protoAsText[msgIdx] == '\n';
            return res;
        }
        
        private string ParseName()
        {
            int idx = _index;
            // Look backward for newline or message beginning
            int start = idx -= 1; // skip whitespace for message or colon for attribute
            while (start > 0 && _protoAsText[start - 1] != ' ' && _protoAsText[start - 1] != '\n')
            {
                start--;
            }

            _name = _protoAsText.Substring(start, idx - start);
            return _name;
        }
    }

    public class ProtoMessage2 : IProtoMessage<ProtoMessage2>
    {
        private readonly List<MsgMatrixElement> _matrix = new List<MsgMatrixElement>();
        private AttrMatrixElement[] _matrixAttrs;
        private string _protoAsText;
        private readonly int _level = 1;
        private readonly int _indexInMatrix;

        private ProtoMessage2(List<MsgMatrixElement> matrix, ref AttrMatrixElement[] attrMatrix, int level, 
            string protoAsText, int indexInMatrix)
        {
            _matrix = matrix;
            _matrixAttrs = attrMatrix;
            _level = level;
            _protoAsText = protoAsText;
            _indexInMatrix = indexInMatrix;
        }

        public void Parse(string protoAsText)
        {
            _protoAsText = protoAsText;
            _matrixAttrs = new AttrMatrixElement[_protoAsText.Length / 4];  // minimal possible attribute has 4 chars
            int currAttrIdx = 0;
            int currentLevel = 0;
            var root = new MsgMatrixElement(0, null, protoAsText);
            _matrix.Add(root);
            bool prevColon = false; // to process colons in string attributes 
            var currentParentMessages = new int?[10]; // [level]: indexInMatrix
            for (int i = 0; i < currentParentMessages.Length; i++)
            {
                currentParentMessages[i] = null;
            }
            
            for (int i = 0; i < _protoAsText.Length; i++)
            {
                char c = _protoAsText[i];
                switch (c)
                {
                    case ':' when !prevColon:
                        _matrixAttrs[currAttrIdx] = new AttrMatrixElement(i, currentLevel, _protoAsText);
                        
                        if (currentParentMessages[currentLevel] != null)
                        {
                            _matrix[(int) currentParentMessages[currentLevel]].AttributeIndexes.Add(currAttrIdx);
                        }
                        else
                        {
                            root.AttributeIndexes.Add(currAttrIdx);
                        }

                        currAttrIdx++;
                        prevColon = true;
                        break;
                    case '{' when !prevColon:
                        currentLevel++;
                        _matrix.Add(new MsgMatrixElement(i, currentLevel, _protoAsText));
                        if (currentParentMessages[currentLevel - 1] != null)
                        {
                            _matrix[(int) currentParentMessages[currentLevel - 1]].ChildIndexes.Add(_matrix.Count - 1);
                        }
                        else
                        {
                            root.ChildIndexes.Add(_matrix.Count - 1);
                        }

                        currentParentMessages[currentLevel] = _matrix.Count - 1;
                        break;
                    case '}' when !prevColon:
                        currentLevel--;
                        break;
                    case '\n':
                        prevColon = false;
                        break;
                }
            }
        }

        public ProtoMessage2()
        {
        }

        public List<ProtoMessage2> GetElementList(string name)
        {
            var res = new List<ProtoMessage2>();
            foreach (int idx in _matrix[_indexInMatrix].ChildIndexes)
            {
                if (_matrix[idx].CheckName(name))
                {
                    res.Add(new ProtoMessage2(_matrix, ref _matrixAttrs, _level + 1, _protoAsText, idx));
                }
            }

            return res;
        }

        public ProtoMessage2 GetElement(string name)
        {
            foreach (int idx in _matrix[_indexInMatrix].ChildIndexes)
            {
                if (_matrix[idx].CheckName(name))
                {
                    return new ProtoMessage2(_matrix, ref _matrixAttrs, _level + 1, _protoAsText, idx);
                }
            }

            return null;
        }

        public List<string> GetAttributeList(string name)
        {
            var res = new List<string>();
            foreach (int i in _matrix[_indexInMatrix].AttributeIndexes)
            {
                if (_matrixAttrs[i].CheckName(name))
                {
                    res.Add(_matrixAttrs[i].Value);
                }
            }

            return res;
        }

        public T GetAttribute<T>(string name) where T : struct
        {
            throw new NotImplementedException();
        }

        public T? GetAttributeOrNull<T>(string name) where T : struct
        {
            string attr = GetAttribute(name);
            return (T) Convert.ChangeType(attr, typeof(T), CultureInfo.InvariantCulture);
        }

        public string GetAttribute(string name)
        {
            foreach (int idx in _matrix[_indexInMatrix].AttributeIndexes)
            {
                if (_matrixAttrs[idx].CheckName(name))
                {
                    return _matrixAttrs[idx].Value;
                }
            }

            return null;
        }

        public List<string> GetKeys() // TODO: check usage. I doubt it really should return a LIST of ALL sub messages 
        {
            var res = new List<string>();

            for (int i = 0; i < _matrix.Count; i++)
            {
                MsgMatrixElement el = _matrix[i];
                if (el.Level >= _level)
                {
                    res.Add(el.Name);
                }
            }

            return res;
        }
    }
}