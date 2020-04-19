using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProtoMessageOriginal
{
    public enum MsgMatrixElementType
    {
        MessageStart = '{',
        MessageEnd = '}',
        Attribute = ':'
    }

    public struct MsgMatrixElement
    {
        public readonly MsgMatrixElementType Type;
        private readonly int _index;  // global "position" in text, '{' for message and ':' for attribute
        public readonly int Level;  // increases on each '{' decreases on each '}'
        public readonly List<int> ChildIndexes;  // sub-messages in matrix
        public readonly List<int> AttributeIndexes;  // attribute index in matrix
        public string? Name => _name ?? ParseName();
        public string Value => _value ?? ParseAttributeValue();
        private string? _value;
        private string? _name;
        private readonly string _protoAsText;
        
        public MsgMatrixElement(MsgMatrixElementType type, int index, int level, string protoAsText)
        {
            Type = type;
            _index = index;
            Level = level;
            ChildIndexes = new List<int>();
            AttributeIndexes = new List<int>();
            _value = null;
            _name = null;
            _protoAsText = protoAsText;
        }

        // For debug purposes
        public override string ToString()
        {
            return $"Index: {_index} Level: {Level} Type: {Type.ToString()} " +
                   $"ChildIndexes: {string.Join(", ", ChildIndexes)} " +
                   $"AttributeIndexes: {string.Join(", ", AttributeIndexes)}";
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

            if (Type == MsgMatrixElementType.Attribute)
            {
                idx++;
            }
            _name = _protoAsText.Substring(start, idx - start);
            return _name;
        }
        
        private string ParseAttributeValue()
        {
            if (Type != MsgMatrixElementType.Attribute)
            {
                return null;
            }
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

    public class ProtoMessage2 : IProtoMessage<ProtoMessage2>
    {
        private readonly List<MsgMatrixElement> _matrix = new List<MsgMatrixElement>();
        private readonly int _matrixStart;
        private int _matrixEnd;
        private string _protoAsText;
        private readonly int _level = 1;
        private readonly int? _indexInMatrix = null;

        private ProtoMessage2(List<MsgMatrixElement> matrix, int matrixStart, int matrixEnd, int level,
            string protoAsText, int indexInMatrix)
        {
            _matrix = matrix;
            _matrixStart = matrixStart;
            _matrixEnd = matrixEnd;
            _level = level;
            _protoAsText = protoAsText;
            _indexInMatrix = indexInMatrix;
        }
        
        public void Parse(string protoAsText)
        {
            _protoAsText = protoAsText;
            int currentLevel = 0;
            bool prevColon = false; // to process colons in string attributes 
            var currentParentMessages = new Dictionary<int, int>();  // level: indexInMatrix
            for (int i = 0; i < _protoAsText.Length; i++)
            {
                char c = _protoAsText[i];
                int indexInMatrix;
                switch (c)
                {
                    case ':' when !prevColon:
                        _matrix.Add(new MsgMatrixElement(MsgMatrixElementType.Attribute, i, currentLevel, _protoAsText));
                        if (currentParentMessages.TryGetValue(currentLevel, out indexInMatrix))
                        {
                            _matrix[indexInMatrix].AttributeIndexes.Add(_matrix.Count - 1);
                        }
                        prevColon = true;
                        break;
                    case '{' when !prevColon:
                        currentLevel++;
                        _matrix.Add(new MsgMatrixElement(MsgMatrixElementType.MessageStart, i, currentLevel, _protoAsText));
                        if (currentParentMessages.TryGetValue(currentLevel - 1, out indexInMatrix))
                        {
                            _matrix[indexInMatrix].ChildIndexes.Add(_matrix.Count - 1);
                        }
                        currentParentMessages[currentLevel] = _matrix.Count - 1;
                        break;
                    case '}' when !prevColon:
                        _matrix.Add(new MsgMatrixElement(MsgMatrixElementType.MessageEnd, i, currentLevel, _protoAsText));
                        currentLevel--;
                        break;
                    case '\n':
                        prevColon = false;
                        break;
                }
            }

            _matrixEnd = _matrix.Count - 1;
        }

        public ProtoMessage2()
        {
        }

        public List<ProtoMessage2> GetElementList(string name)
        {
            var res = new List<ProtoMessage2>();
            if (_indexInMatrix == null)  // TODO: I really need a root message
            {
                for (int index = 0; index < _matrix.Count; index++)
                {
                    MsgMatrixElement el = _matrix[index];
                    if (el.Level == _level && el.Type == MsgMatrixElementType.MessageStart && el.Name == name)
                    {
                        res.Add(new ProtoMessage2(_matrix, _matrixStart, _matrixEnd, _level + 1, _protoAsText, index));
                    }
                }

                return res;
            }
            
            // if we does know submessage
            foreach (int idx in _matrix[(int) _indexInMatrix].ChildIndexes)
            {
                if (_matrix[idx].Name == name)
                {
                    res.Add(new ProtoMessage2(_matrix, _matrixStart, _matrixEnd, _level + 1, _protoAsText, idx));
                }
            }

            return res;
        }

        public ProtoMessage2 GetElement(string name)
        {
            for (int i = 0; i < _matrix.Count; i++)
            {
                MsgMatrixElement el = _matrix[i];
                if (el.Type != MsgMatrixElementType.MessageStart || el.Level != _level || el.Name != name)
                {
                    continue;
                }

                return new ProtoMessage2(_matrix, _matrixStart, _matrixEnd, _level + 1, _protoAsText, i);
            }

            return null;
        }

        public List<string> GetAttributeList(string name)
        {
            var res = new List<string>();

            if (_indexInMatrix == null)  // root message case
            {
                foreach (MsgMatrixElement el in _matrix)
                {
                    if (el.Level == _level && el.Type == MsgMatrixElementType.Attribute && el.Name == name)
                    {
                        res.Add(el.Value);
                    }
                }

                return res;
            }
            
            // nested message case
            foreach (int i in _matrix[(int) _indexInMatrix].AttributeIndexes)
            {
                if (_matrix[i].Name == name)
                {
                    res.Add(_matrix[i].Value);
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
            if (_indexInMatrix != null)  // We does know children
            {
                foreach (int idx in _matrix[(int) _indexInMatrix].AttributeIndexes)
                {
                    if (_matrix[idx].Name == name)
                    {
                        return _matrix[idx].Value;
                    }
                }

                return null;
            }
            // we're have to find root message attribute
            foreach (MsgMatrixElement el in _matrix)
            {
                if (el.Level != _level || el.Type != MsgMatrixElementType.Attribute || el.Name != name)
                {
                    continue;
                }

                return el.Value;
            }

            return null;
        }

        public List<string> GetKeys() // TODO: check usage. I doubt it really should return a LIST of ALL sub messages 
        {
            var res = new List<string>();

            for (int i = _matrixStart; i < _matrixEnd; i++)
            {
                MsgMatrixElement el = _matrix[i];
                if (el.Type == MsgMatrixElementType.MessageStart && el.Level >= _level)
                {
                    res.Add(el.Name);
                }
            }

            return res;
        }
    }
}