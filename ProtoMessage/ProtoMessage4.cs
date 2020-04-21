using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProtoMessageOriginal
{

    public class ProtoMessage4 : IProtoMessage<ProtoMessage4>
    {
        private List<(LazyString, LazyString)> _attributes = new List<(LazyString, LazyString)>();
        private List<(LazyString, ProtoMessage4)> _subMessages = new List<(LazyString, ProtoMessage4)>();


        private readonly Dictionary<string, string> _attributesCache = new Dictionary<string, string>();
        private readonly Dictionary<string, ProtoMessage4> _subMessagesCache = new Dictionary<string, ProtoMessage4>();

        private string _protoAsText;

        public ProtoMessage4()
        {
        }

        public ProtoMessage4(string protoAsText)
        {
            _protoAsText = protoAsText;
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

            List<(LazyString, LazyString)> currentMessageAttributes = currentProtoMessage._attributes;
            List<(LazyString, ProtoMessage4)> currentMessagSubMessages = currentProtoMessage._subMessages;

            var elementNameStartPosition = 0;

            var attributeNameStartPosition = 0;
            LazyString attributeName = LazyString.Empty;

            var attributeValueStartPosition = 0;

            char char1;

            bool isLineStart = true;
            bool attributeValue = false;

            for (int i = 0; i < lengthToProcess; i++)
            {
                char1 = protoAsText[i];

                if (!attributeValue)
                {
                    if (char1 == ' ' && isLineStart)
                    {
                        attributeNameStartPosition = i + 1;
                        elementNameStartPosition = i + 1;

                        continue;
                    }
                    else if (char1 == ':')
                    {
                        attributeName = new LazyString(attributeNameStartPosition, i);

                        attributeValueStartPosition = i + 1;
                        attributeValue = true;

                        continue;
                    }
                    else if (char1 == '{')
                    {
                        messagesStack.Push(currentProtoMessage);
                        currentProtoMessage = new ProtoMessage4(protoAsText);
                        currentMessagSubMessages.Add((
                            new LazyString(elementNameStartPosition, i - 1),
                            currentProtoMessage));

                        currentMessageAttributes = currentProtoMessage._attributes;
                        currentMessagSubMessages = currentProtoMessage._subMessages;

                        continue;
                    }
                    else if (char1 == '}')
                    {
                        currentProtoMessage = messagesStack.Pop();
                        currentMessageAttributes = currentProtoMessage._attributes;
                        currentMessagSubMessages = currentProtoMessage._subMessages;

                        continue;
                    }

                    else
                    {
                        isLineStart = false;
                    }
                }

                if (char1 == '\n')
                {
                    if (!attributeName.isEqual(LazyString.Empty))
                    {
                        currentMessageAttributes.Add((attributeName, new LazyString(attributeValueStartPosition, i)));
                        attributeName = LazyString.Empty;
                    }

                    attributeNameStartPosition = i + 1;
                    elementNameStartPosition = i + 1;

                    isLineStart = true;
                    attributeValue = false;
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

            var value = _protoAsText.Substring(start, lazyString._indexEnd - start);
            if (value[value.Length - 1] == '\r')
            {
                value = value.Substring(0, value.Length - 1);
            }
            if (value[0] == '"')
            {
                value = value.Substring(1, value.Length - 2);
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
            for(int i = 0; i < _attributes.Count; i++)
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

            void GetSubMessages(List<(LazyString, ProtoMessage4)> messages)
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