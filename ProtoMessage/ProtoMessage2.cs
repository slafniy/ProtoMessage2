using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProtoMessageOriginal
{
    public struct Attribute
    {
        private readonly int _index; // global "position" in text, '{' for message and ':' for attribute
        private readonly int? _level; // increases on each '{' decreases on each '}'
        public string Value => _value ?? ParseAttributeValue();
        private string? _value;
        private readonly string _protoAsText;

        public Attribute(int index, int? level, string protoAsText)
        {
            _index = index;
            _level = level;
            _value = null;
            _protoAsText = protoAsText;
        }

        // For debug purposes
        public override string ToString()
        {
            return $"Index: {_index} Level: {_level} ";
        }

        // Do not save the result! Substring is too slow!
        public bool CheckName(string name)
        {
            int nameIdx = name.Length - 1; // the end of the name
            int attrIdx = _index - 1; // skip colon

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

            // This works faster than .Trim()
            if (_protoAsText[start] == '"')
            {
                start++;
                index--;
            }

            _value = _protoAsText.Substring(start, index - start);
            return _value;
        }
    }


    public struct SubMessage
    {
        private readonly int _index; // global "position" in text, index of '{' symbol
        public readonly int? Level; // How deep this message is. (increases on each '{' decreases on each '}')
        public readonly List<int> ChildIndexes; // sub-message indexes in sub-messages matrix
        public readonly List<int> AttributeIndexes; // attribute indexes in attributes matrix
        public string? Name => _name ?? ParseName();
        private string? _name;
        private readonly string _protoAsText;

        public SubMessage(int index, int? level, string protoAsText)
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
            int nameIdx = name.Length - 1; // the end of the name
            int msgIdx = _index - 2; // skip '{' and whitespace

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

            return msgIdx < 0 || _protoAsText[msgIdx] == ' ' || _protoAsText[msgIdx] == '\n';
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
        // These fields are same for every sub-ProtoMessage2
        private readonly List<SubMessage> _subMessagesMatrix = new List<SubMessage>(); // all levels sub-messages
        private Attribute[] _matrixAttributes; // all levels attributes
        private string _protoAsText; // original input string

        // These fields determine particular (sub) ProtoMessage instance
        private readonly int _level = 1; // how deep this message is  TODO: probably needed only for GetKeys()
        private readonly int _indexInMatrix; // where in the common matrix is this message data

        // Called only to return submessage TODO: maybe it would be better to have xPath-like access?
        private ProtoMessage2(List<SubMessage> matrix, ref Attribute[] attrMatrix, int level, string protoAsText,
            int indexInMatrix)
        {
            _subMessagesMatrix = matrix;
            _matrixAttributes = attrMatrix;
            _protoAsText = protoAsText;
            _level = level;
            _indexInMatrix = indexInMatrix;
        }

        // Should be called once after instance creation
        public void Parse(string protoAsText)
        {
            _protoAsText = protoAsText;
            _matrixAttributes = new Attribute[_protoAsText.Length / 5]; // minimal possible attribute has 5 chars
            _subMessagesMatrix.Add(new SubMessage(0, null, protoAsText)); // contains possible several 0-level messages

            int currentAttributeIndex = 0;
            int currentLevel = 0;
            bool isPreviousColon = false; // to process colons inside attribute values

            var currentParentMessages = new Stack<int>(); //  saves parent messages (their indexes in matrix)
            currentParentMessages.Push(0);

            for (int i = 0; i < _protoAsText.Length; i++)
            {
                char c = _protoAsText[i];
                switch (c)
                {
                    case ':' when !isPreviousColon:
                        _matrixAttributes[currentAttributeIndex] = new Attribute(i, currentLevel, _protoAsText);
                        _subMessagesMatrix[currentParentMessages.Peek()].AttributeIndexes.Add(currentAttributeIndex);
                        currentAttributeIndex++;
                        isPreviousColon = true;
                        break;
                    case '{' when !isPreviousColon:
                        currentLevel++;
                        _subMessagesMatrix.Add(new SubMessage(i, currentLevel, _protoAsText));
                        _subMessagesMatrix[currentParentMessages.Peek()].ChildIndexes.Add(_subMessagesMatrix.Count - 1);
                        currentParentMessages.Push(_subMessagesMatrix.Count - 1);
                        break;
                    case '}' when !isPreviousColon:
                        currentLevel--;
                        currentParentMessages.Pop();
                        break;
                    case '\n':
                        isPreviousColon = false;
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
            foreach (int idx in _subMessagesMatrix[_indexInMatrix].ChildIndexes)
            {
                if (_subMessagesMatrix[idx].CheckName(name))
                {
                    res.Add(new ProtoMessage2(_subMessagesMatrix, ref _matrixAttributes, _level + 1, _protoAsText,
                        idx));
                }
            }

            return res;
        }

        public ProtoMessage2 GetElement(string name)
        {
            foreach (int idx in _subMessagesMatrix[_indexInMatrix].ChildIndexes)
            {
                if (_subMessagesMatrix[idx].CheckName(name))
                {
                    return new ProtoMessage2(_subMessagesMatrix, ref _matrixAttributes, _level + 1, _protoAsText, idx);
                }
            }

            return null;
        }

        public List<string> GetAttributeList(string name)
        {
            var res = new List<string>();
            foreach (int i in _subMessagesMatrix[_indexInMatrix].AttributeIndexes)
            {
                if (_matrixAttributes[i].CheckName(name))
                {
                    res.Add(_matrixAttributes[i].Value);
                }
            }

            return res;
        }

        public T GetAttribute<T>(string name) where T : struct
        {
            string attr = GetAttribute(name);
            return (T) Convert.ChangeType(attr, typeof(T), CultureInfo.InvariantCulture);
        }

        // TODO: remove? It looks same as GetAttribute() when using nullable reference types
        public T? GetAttributeOrNull<T>(string name) where T : struct
        {
            string attr = GetAttribute(name);
            return (T) Convert.ChangeType(attr, typeof(T), CultureInfo.InvariantCulture);
        }

        public string GetAttribute(string name)
        {
            foreach (int idx in _subMessagesMatrix[_indexInMatrix].AttributeIndexes)
            {
                if (_matrixAttributes[idx].CheckName(name))
                {
                    return _matrixAttributes[idx].Value;
                }
            }

            return null;
        }

        public List<string> GetKeys() // TODO: check usage. I doubt it really should return a LIST of ALL sub messages 
        {
            var res = new List<string>();

            foreach (SubMessage el in _subMessagesMatrix)
            {
                if (el.Level >= _level)
                {
                    res.Add(el.Name);
                }
            }

            return res;
        }
    }
}