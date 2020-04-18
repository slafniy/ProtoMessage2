using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
        public readonly int Index;  // global "position" in text, '{' for message and ':' for attribute
        public readonly int Level;  // increases on each '{' decreases on each '}'

        public MsgMatrixElement(MsgMatrixElementType type, int index, int level)
        {
            Type = type;
            Index = index;
            Level = level;
        }

        // For debug purposes
        public override string ToString()
        {
            return $"Index: {Index} Level: {Level} Type: {Type.ToString()}";
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
        private readonly int _level = 1;
        private bool _isParsed;
        
        private readonly Fields<Attribute> _attributes = new Fields<Attribute>();
        private readonly Fields<ProtoMessage2> _subMessages = new Fields<ProtoMessage2>();

        private ProtoMessage2(List<MsgMatrixElement> matrix, int level, string protoAsText)
        {
            _matrix = matrix;
            _level = level;
            _protoAsText = protoAsText;
        }

        private void ParseCurrentLevel()
        {
            if (_isParsed)
            {
                return;
            }

            _isParsed = true;
            
            int msgStartPos = 0;
            for (int i = 0; i < _matrix.Count; i++)
            {
                MsgMatrixElement el = _matrix[i];
                if (el.Type == MsgMatrixElementType.MessageStart && el.Level == _level)
                {
                    msgStartPos = i;
                }

                // We've found the end of current message
                if (el.Type == MsgMatrixElementType.MessageEnd && el.Level == _level)
                {
                    _subMessages.AddField(GetName(_matrix[msgStartPos].Index),
                        new ProtoMessage2(_matrix.GetRange(msgStartPos + 1, i - msgStartPos - 1), 
                            _level + 1, _protoAsText));
                }

                if (el.Level == _level - 1 && el.Type == MsgMatrixElementType.Attribute)
                {
                    ParseAttribute(el.Index);
                }
            }
        }

        private class Attribute
        {
            private readonly int _index;
            private readonly string? _protoAsText;
            private string? _value;

            public string Value => _value ?? ParseAttributeValue();

            public Attribute(int idx, string protoAsText)
            {
                _index = idx;
                _protoAsText = protoAsText;
            }

            private string ParseAttributeValue()
            {
                int index = _index;
                int start = index + 1; // skip whitespace
                while (index < _protoAsText.Length && _protoAsText[index] != '}' && _protoAsText[index] != '\n')
                {
                    index++;
                }
            
                _value = _protoAsText.Substring(start, index - start).Trim('"');
                return _value;
            }
        }
        
        private void ParseAttribute(int index)
        {
            index++;  // Why? Because GetName() does "-1", and I need this anyway to find a value
            string name = GetName(index);

            _attributes.AddField(name, new Attribute(index, _protoAsText));
        }
        
        private string GetName(int endIdx)
        {
            int idx = endIdx;
            // Look backward for newline or message beginning
            int start = idx -= 1;  // skip whitespace for message or colon for attribute
            while (start > 0 && _protoAsText[start - 1] != ' ' && _protoAsText[start - 1] != '\n')
            {
                start--;
            }

            return _protoAsText.Substring(start, idx - start);
        }

        public void Parse(string protoAsText)
        {
            _protoAsText = protoAsText;
            int currentLevel = 0;
            for (int i = 0; i < _protoAsText.Length; i++)
            {
                char c = _protoAsText[i];
                switch (c)
                {
                    case ':':
                        _matrix.Add(
                            new MsgMatrixElement(MsgMatrixElementType.Attribute, i, currentLevel));
                        break;
                    case '{':
                        currentLevel++;
                        _matrix.Add(new MsgMatrixElement(MsgMatrixElementType.MessageStart, i, currentLevel));
                        break;
                    case '}':
                        _matrix.Add(new MsgMatrixElement(MsgMatrixElementType.MessageEnd, i, currentLevel));
                        currentLevel--;
                        break;
                }
            }
        }

        public ProtoMessage2()
        {
        }

        public List<ProtoMessage2> GetElementList(string name)
        {
            ParseCurrentLevel();
            return _subMessages.ContainsKey(name) ? _subMessages[name] : new List<ProtoMessage2>();
        }

        public ProtoMessage2 GetElement(string name)
        {
            ParseCurrentLevel();
            return _subMessages.ContainsKey(name) && _subMessages[name].Count > 0 ? _subMessages[name][0] : null;
        }

        public List<string> GetAttributeList(string name)
        {
            ParseCurrentLevel();
            return !_attributes.ContainsKey(name) ? new List<string>() : _attributes[name].Select(attr => attr.Value).ToList();
        }

        public T GetAttribute<T>(string name) where T : struct
        {
            ParseCurrentLevel();
            string attr = GetAttribute(name);
            return (T) Convert.ChangeType(attr, typeof(T), CultureInfo.InvariantCulture);
        }

        public T? GetAttributeOrNull<T>(string name) where T : struct
        {
            string attr = GetAttribute(name);
            if (attr == null)
            {
                return null;
            }

            return (T) Convert.ChangeType(attr, typeof(T), CultureInfo.InvariantCulture);
        }

        public string GetAttribute(string name)
        {
            ParseCurrentLevel();
            // TODO: Yes, it really should work in this way! I don't like this logic. Same for GetElement
            // TODO: I'd return null in case of repeated message
            return _attributes.ContainsKey(name) && _attributes[name].Count > 0 ? _attributes[name][0].Value : null;
        }

        public List<string> GetKeys()  // TODO: it should return data from sub-messages too!
        {
            ParseCurrentLevel();
            return (from kv in _subMessages from _ in kv.Value select kv.Key).ToList();
        }
    }
}