using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProtoMessageOriginal
{
    public interface IProtoMessage<TType> where TType : IProtoMessage<TType>, new()
    {
        List<TType> GetElementList(string name);
        TType GetElement(string name);
        List<string> GetAttributeList(string name);
        T GetAttribute<T>(string name) where T : struct;
        T? GetAttributeOrNull<T>(string name) where T : struct;
        string GetAttribute(string name);
        void Parse(string text);
        List<string> GetKeys();
    }

    public class ProtoMessage : IProtoMessage<ProtoMessage>
    {
        private readonly List<string> _text = new List<string>();
        private readonly Dictionary<char, List<string>> _mNonParsedAttributes = new Dictionary<char, List<string>>();
        private readonly Dictionary<string, List<string>> _mAttributes = new Dictionary<string, List<string>>();
        private readonly List<string> _mKeys = new List<string>();

        private readonly Dictionary<string, List<ProtoMessage>> _mElements =
            new Dictionary<string, List<ProtoMessage>>();

        private List<string> _nonParsedList;
        private List<string> _outList; // just lists for different processes with TryGetValue
        private List<ProtoMessage> _protoList;

        public List<string> GetAttributeList(string name)
        {
            if (_text.Count != 0)
            {
                ParseMessageBody(_text.ToArray());
                _text.Clear();
            }

            if (_mAttributes.TryGetValue(name, out _outList))
            {
                return _mAttributes.TryGetValue(name, out _outList) ? _outList : new List<string>();
            }

            var attributes = ParseAndReturnAttributes(name);
            _mAttributes[name] = attributes;

            return _mAttributes.TryGetValue(name, out _outList) ? _outList : new List<string>();
        }

        private List<string> ParseAndReturnAttributes(string name)
        {
            char firstChar = name[0];
            if (!_mNonParsedAttributes.TryGetValue(firstChar, out _nonParsedList)) return new List<string>();

            var nonParsedAttributes = _nonParsedList.Where(
                a => a.Length > name.Length + 2 && a.Substring(0, name.Length) == name).ToArray();

            _outList = new List<string>();
            for (int i = nonParsedAttributes.Length - 1; i > -1; i--)
            {
                string nonParsedAttribute = nonParsedAttributes[i];
                string attribute = parseAttribute(nonParsedAttribute, name.Length);
                _nonParsedList.Remove(nonParsedAttribute);
                _outList.Add(attribute);
            }

            _outList.Reverse();
            _mAttributes.Add(name, _outList);
            return _outList;
        }

        public T GetAttribute<T>(string name) where T : struct
        {
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
            if (_mAttributes.TryGetValue(name, out _outList)) return _outList.Count > 0 ? _outList[0] : null;

            if (_text.Count != 0)
            {
                ParseMessageBody(_text.ToArray());
                _text.Clear();
            }

            if (!_mNonParsedAttributes.TryGetValue(name[0], out _outList)) return null;

            var attr = parseAttribute(_outList.Find(a => a.Length > name.Length + 2 &&
                                                         a.Substring(0, name.Length) == name), name.Length);

            _mNonParsedAttributes[name[0]].Remove(attr);
            _mAttributes.Add(name, new List<string> {attr});
            return attr;
        }

        private string parseAttribute(string nonParsedAttribute, int idx)
        {
            if (nonParsedAttribute == null) return null;
            idx += 2;

            if (nonParsedAttribute[idx] == '\"')
            {
                return nonParsedAttribute.Substring(idx + 1, nonParsedAttribute.Length - idx - 2);
            }

            return nonParsedAttribute.Substring(idx, nonParsedAttribute.Length - idx);
        }

        public List<ProtoMessage> GetElementList(string name)
        {
            if (_text.Count != 0)
            {
                ParseMessageBody(_text.ToArray());
                _text.Clear();
            }

            if (_mElements.TryGetValue(name, out _protoList))
            {
                return _protoList;
            }

            return new List<ProtoMessage>();
        }

        public ProtoMessage GetElement(string name)
        {
            _protoList = GetElementList(name);
            return _protoList.Count > 0 ? _protoList[0] : null;
        }

        public List<string> GetKeys()
        {
            return _mKeys;
        }

        public void Parse(string text)
        {
            ParseMessageBody(text.Split('\n'));
        }


        private void ParseMessageBody(string[] lines)
        {
            bool isTopLevel = false; // level 0 
            string protoName = String.Empty;
            ProtoMessage otherProto = new ProtoMessage(); // ProtoMessage level > 0

            foreach (var line in lines.Where(l => l != "")) // last line == "", when parse "msg" attribute
            {
                if (line.Length == 1) // where line == "}" (first level, without spaces)
                {
                    isTopLevel = false;
                    if (!_mElements.ContainsKey(protoName))
                    {
                        _mElements.Add(protoName, new List<ProtoMessage>());
                    }

                    _mElements[protoName].Add(otherProto);
                    otherProto = new ProtoMessage();
                    continue;
                }

                if (isTopLevel) // if level > 0
                {
                    otherProto._text.Add(line.Substring(2));
                    if (line[line.Length - 1] == '{') // start of the element
                    {
                        _mKeys.Add(line.Substring(0, line.Length - 2).Trim());
                    }
                }
                else // if level == 0
                {
                    if (line[line.Length - 1] == '{') // start of the element
                    {
                        protoName = line.Substring(0, line.Length - 2).Trim(); // without " {"
                        isTopLevel = true;
                        _mKeys.Add(protoName);
                    }
                    else // attribute
                    {
                        if (!_mNonParsedAttributes.ContainsKey(line[0]))
                        {
                            _mNonParsedAttributes.Add(line[0], new List<string>());
                        }

                        _mNonParsedAttributes[line[0]].Add(line);
                    }
                }
            }
        }
    }
}