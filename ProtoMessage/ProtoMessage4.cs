using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProtoMessage
{

    public class ProtoMessage4 : IProtoMessage<ProtoMessage4>
    {
        private List<LazyStringTuple> _attributes = new List<LazyStringTuple>();
        private List<LazyStringProtoMessage4Tuple> _subMessages = new List<LazyStringProtoMessage4Tuple>();

        private string _protoAsText;

        public ProtoMessage4()
        {
        }

        public ProtoMessage4(string protoAsText)
        {
            _protoAsText = protoAsText;
        }

        private struct LazyStringTuple
        {
            public readonly LazyString Item1;
            public readonly LazyString Item2;

            public LazyStringTuple(LazyString item1, LazyString item2)
            {
                Item1 = item1;
                Item2 = item2;
            }
        }


        private struct LazyStringProtoMessage4Tuple
        {
            public readonly LazyString Item1;
            public readonly ProtoMessage4 Item2;

            public LazyStringProtoMessage4Tuple(LazyString item1, ProtoMessage4 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }
        }

        private struct LazyString
        {
            public readonly int _indexStart;
            public readonly int _indexEnd;

            public string Value;

            public LazyString(int indexStart, int indexEnd)
            {
                _indexStart = indexStart;
                _indexEnd = indexEnd;
                Value = null;
            }

            public static LazyString Empty = new LazyString(-1, -1);

            public bool isEqual(LazyString lazyStringd)
            {
                return lazyStringd._indexStart == _indexStart && lazyStringd._indexEnd == _indexEnd;
            }
        }

        public void Parse(string protoAsText)
        {
            _protoAsText = protoAsText;
            var lengthToProcess = protoAsText.Length;

            Stack<ProtoMessage4> messagesStack = new Stack<ProtoMessage4>();
            var currentProtoMessage = this;

            List<LazyStringTuple> currentMessageAttributes = currentProtoMessage._attributes;
            List<LazyStringProtoMessage4Tuple> currentMessagSubMessages = currentProtoMessage._subMessages;

            var elementNameStartPosition = 0;

            var attributeNameStartPosition = 0;
            LazyString attributeName = LazyString.Empty;

            var attributeValueStartPosition = 0;

            char char1;

            int isLineStart = 0;
            int attributeValue = 1;

            var spaceMask = ~' ';

            for (int i = 0; i < lengthToProcess; i++)
            {
                char1 = protoAsText[i];

                switch ((attributeValue << 17) | char1)
                {
                    // ' '				(attributeValue << 17) |' '	131104	int
                    case 131104:
                        if (isLineStart == 0)
                        {
                            attributeNameStartPosition = i + 1;
                            elementNameStartPosition = i + 1;
                        }
                        break;

                    // ':'		(attributeValue << 17) |':'	131130	int
                    case 131130:
                        attributeName = new LazyString(attributeNameStartPosition, i);

                        attributeValueStartPosition = i + 1;
                        attributeValue = 0;

                        isLineStart = char1 & spaceMask;
                        break;

                    // '{'		(attributeValue << 17) | '{'	131195	int
                    case 131195:
                        messagesStack.Push(currentProtoMessage);
                        currentProtoMessage = new ProtoMessage4(protoAsText);
                        currentMessagSubMessages.Add(new LazyStringProtoMessage4Tuple(
                            new LazyString(elementNameStartPosition, i - 1),
                            currentProtoMessage));

                        currentMessageAttributes = currentProtoMessage._attributes;
                        currentMessagSubMessages = currentProtoMessage._subMessages;

                        isLineStart = char1 & spaceMask;
                        break;

                    // '}' 		(attributeValue << 17) | '}'	131197	int
                    case 131197:
                        currentProtoMessage = messagesStack.Pop();
                        currentMessageAttributes = currentProtoMessage._attributes;
                        currentMessagSubMessages = currentProtoMessage._subMessages;

                        isLineStart = char1 & spaceMask;
                        break;

                    // '\n' 		10
                    // '\n' 		(attributeValue << 17) | '\n'	131082	int
                    case 10:
                    case 131082:
                        if (!attributeName.isEqual(LazyString.Empty))
                        {
                            currentMessageAttributes.Add(new LazyStringTuple(attributeName, new LazyString(attributeValueStartPosition, i)));
                            attributeName = LazyString.Empty;
                        }

                        attributeNameStartPosition = i + 1;
                        elementNameStartPosition = i + 1;

                        isLineStart = 0;
                        attributeValue = 1;

                        break;
                    default:

                        isLineStart = char1 & spaceMask;
                        break;
                }

            }
        }

        private string ParseLazyStringValue(LazyString lazyString)
        {
            if (lazyString.Value != null)
            {
                return lazyString.Value;
            }

            int start = lazyString._indexStart;

            lazyString.Value = _protoAsText.Substring(start, lazyString._indexEnd - start);
            return lazyString.Value;
        }

        private string ParseAttributeValue(LazyString lazyString)
        {
            if (lazyString.Value != null)
            {
                return lazyString.Value;
            }

            int start = lazyString._indexStart + 1; // skip whitespace

            string value;
            if (_protoAsText[start] == '"')
            {
                value = _protoAsText.Substring(start + 1, lazyString._indexEnd - start - 2);
            }
            else
            {
                value = _protoAsText.Substring(start, lazyString._indexEnd - start);
            }

            lazyString.Value = value;
            return value;
        }

        private bool AreEqual(LazyString lazyString, string anotherString)
        {
            int i = 0;
            int j = lazyString._indexStart;
            int length = anotherString.Length;

            if (lazyString._indexEnd - j != length)
            {
                return false;
            }

            while (i < length)
            {
                if (anotherString[i] != _protoAsText[j])
                {
                    return false;
                }

                i++;
                j++;
            }

            return true;
        }

        public List<ProtoMessage4> GetElementList(string name)
        {
            var res = new List<ProtoMessage4>();
            for (int i = 0; i < _subMessages.Count; i++)
            {
                var subMessages = _subMessages[i];
                if (AreEqual(subMessages.Item1, name))
                {
                    res.Add(subMessages.Item2);
                }
            }
            return res;
        }

        public ProtoMessage4 GetElement(string name)
        {
            for (int i = 0; i < _subMessages.Count; i++)
            {
                var subMessages = _subMessages[i];
                if (AreEqual(subMessages.Item1, name))
                {
                    return subMessages.Item2;
                }
            }
            return null;
        }

        public List<string> GetAttributeList(string name)
        {
            var res = new List<string>();
            for (int i = 0; i < _attributes.Count; i++)
            {
                var attribute = _attributes[i];
                if (AreEqual(attribute.Item1, name))
                {
                    res.Add(ParseAttributeValue(attribute.Item2));
                }
            }
            return res;
        }

        public T GetAttribute<T>(string name) where T : struct
        {
            string attr = GetAttribute(name);
            return (T)Convert.ChangeType(attr, typeof(T), CultureInfo.InvariantCulture);
        }

        public T? GetAttributeOrNull<T>(string name) where T : struct
        {
            string attr = GetAttribute(name);
            if (attr == null)
            {
                return null;
            }

            return (T)Convert.ChangeType(attr, typeof(T), CultureInfo.InvariantCulture);
        }

        public string GetAttribute(string name)
        {
            for (int i = 0; i < _attributes.Count; i++)
            {
                var attribute = _attributes[i];
                if (AreEqual(attribute.Item1, name))
                {
                    return ParseAttributeValue(attribute.Item2);
                }
            }
            return null;
        }

        public List<string> GetKeys()
        {
            var res = new List<string>();

            void GetSubMessages(List<LazyStringProtoMessage4Tuple> messages)
            {
                foreach (var msg in messages)
                {
                    res.Add(ParseLazyStringValue(msg.Item1));
                    GetSubMessages(msg.Item2._subMessages);
                }
            }

            GetSubMessages(_subMessages);

            return res;
        }
    }
}