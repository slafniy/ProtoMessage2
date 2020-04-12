using System;
using System.Collections.Generic;

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
        public readonly int Index; // global "position" in text, '{' for message and ':' for attribute
        public readonly int Level; // increases on each '{' decreases on each '}'
        public readonly int Number; // increases on each '{', NEVER decreases

        public MsgMatrixElement(MsgMatrixElementType type, int index, int level, int number)
        {
            Type = type;
            Index = index;
            Level = level;
            Number = number;
        }

        public string GetMessageName(string protoAsText)
        {
            int idx = Index;
            // Look backward for newline or message beginning
            int start = idx -= 1; // skip whitespace
            while (start > 0 && protoAsText[start - 1] != ' ' && protoAsText[start - 1] != '\n')
            {
                start--;
            }

            return protoAsText.Substring(start, idx - start);
        }

        // For debug purposes
        public override string ToString()
        {
            return $"Index: {Index} Level: {Level} Number: {Number} Type: {Type.ToString()}";
        }
    }

    public class Fields<T> : Dictionary<string, List<T>>
    {
        public void AddField(string name, T value)
        {
            if (!TryGetValue(name, out List<T> attrsWithGivenName))
            {
                attrsWithGivenName = new List<T>();
                Add(name, attrsWithGivenName);
            }

            attrsWithGivenName.Add(value);
        }
    }


    public class ProtoMessage2 : IProtoMessage<ProtoMessage2>
    {
        private readonly List<MsgMatrixElement> _matrix = new List<MsgMatrixElement>();
        private string _protoAsText;
        private int _level = 1;

        private readonly Fields<string> _attributes = new Fields<string>();
        private readonly Fields<ProtoMessage2> _subMessages = new Fields<ProtoMessage2>();

        private ProtoMessage2(List<MsgMatrixElement> matrix, int level, string protoAsText)
        {
            _matrix = matrix;
            _level = level;
            _protoAsText = protoAsText;
            ParseCurrentLevel();
        }

        private void ParseCurrentLevel()
        {
            int msgStartPos = 0;
            int msgStartNumber = 0;
            for (int i = 0; i < _matrix.Count; i++)
            {
                MsgMatrixElement el = _matrix[i];
                if (el.Type == MsgMatrixElementType.MessageStart && el.Level == _level)
                {
                    msgStartPos = i;
                    msgStartNumber = el.Number;
                }

                // We've found the end of current message
                if (el.Type == MsgMatrixElementType.MessageEnd && el.Level == _level - 1)
                {
                    _subMessages.AddField(_matrix[msgStartPos].GetMessageName(_protoAsText),
                        new ProtoMessage2(_matrix.GetRange(msgStartPos, i - msgStartPos), _level + 1, _protoAsText));
                }
            }
        }

        public void Parse(string protoAsText)
        {
            _protoAsText = protoAsText;
            int currentLevel = 0;
            int currentNumber = 0;
            for (int i = 0; i < _protoAsText.Length; i++)
            {
                char c = _protoAsText[i];
                switch (c)
                {
                    case ':':
                        _matrix.Add(
                            new MsgMatrixElement(MsgMatrixElementType.Attribute, i, currentLevel, currentNumber));
                        break;
                    case '{':
                        currentNumber++;
                        currentLevel++;
                        _matrix.Add(new MsgMatrixElement(MsgMatrixElementType.MessageStart, i, currentLevel,
                            currentNumber));
                        break;
                    case '}':
                        currentLevel--;
                        _matrix.Add(new MsgMatrixElement(MsgMatrixElementType.MessageEnd, i, currentLevel,
                            currentNumber));
                        break;
                }
            }
            ParseCurrentLevel();
        }

        public ProtoMessage2()
        {
        }

        public List<ProtoMessage2> GetElementList(string name)
        {
            return _subMessages.ContainsKey(name) ? _subMessages[name] : null;
        }

        public ProtoMessage2 GetElement(string name)
        {
            return _subMessages.ContainsKey(name) ? _subMessages[name].Count == 1 ? _subMessages[name][0] : null : null;
        }

        public List<string> GetAttributeList(string name)
        {
            throw new NotImplementedException();
        }

        public T GetAttribute<T>(string name) where T : struct
        {
            throw new NotImplementedException();
        }

        public T? GetAttributeOrNull<T>(string name) where T : struct
        {
            throw new NotImplementedException();
        }

        public string GetAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public List<string> GetKeys()
        {
            throw new NotImplementedException();
        }
    }
}