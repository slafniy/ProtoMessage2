using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ProtoMessageOriginal
{
    // NOTE this file is a result of mid-night crazy thoughts, look at your own risk.
    // approaches used here is not show good performance, but possible can be reused with some another one.

    [StructLayout(LayoutKind.Explicit, Size = 8, CharSet = CharSet.Ansi)]
    public struct String4Chars
    {
        [FieldOffset(0)] public char char1;
        [FieldOffset(2)] public char char2;
        [FieldOffset(4)] public char char3;
        [FieldOffset(6)] public char char4;
    }

    public class ProtoMessage3 : IProtoMessage<ProtoMessage3>
    {
        private char[] _protoAsCharArray;

        private readonly Fields<string> _attributes = new Fields<string>();
        private readonly Fields<ProtoMessage3> _subMessages = new Fields<ProtoMessage3>();

        public ProtoMessage3()
        {
        }

        public unsafe void Parse(string protoAsText)
        {
            _protoAsCharArray = protoAsText.ToCharArray();

            var remainder = protoAsText.Length % 4;
            var lengthToProcess = protoAsText.Length - remainder;

            String4Chars string4Chars;

            Stack<ProtoMessage3> messagesStack = new Stack<ProtoMessage3>();
            var currentProtoMessage = this;

            var elementNameStartPosition = 0;
            string elementName = null;

            var attributeNameStartPosition = 0;
            string attributeName = null;

            var attributeValueStartPosition = 0;
            string attributeValue = null;
            int i = 0;

            fixed (char* firstChar = &_protoAsCharArray[0])
            {
                for (; i < lengthToProcess; i = i + 4)
                {
                    fixed (char* packet = &_protoAsCharArray[i])
                    {
                        string4Chars = *(String4Chars*)packet;
                    }

                    if (string4Chars.char1 == '{')
                    {
                        elementName = new string(firstChar, elementNameStartPosition, i - 1 - elementNameStartPosition).Trim();
                        List<ProtoMessage3> submessagesList;
                        if (!currentProtoMessage._subMessages.TryGetValue(elementName, out submessagesList))
                        {
                            submessagesList = new List<ProtoMessage3>();
                            currentProtoMessage._subMessages.Add(elementName, submessagesList);
                        }
                        messagesStack.Push(currentProtoMessage);
                        currentProtoMessage = new ProtoMessage3();
                        submessagesList.Add(currentProtoMessage);
                    }

                    if (string4Chars.char1 == '}')
                    {
                        currentProtoMessage = messagesStack.Pop();
                    }

                    if (string4Chars.char1 == '\n')
                    {
                        attributeNameStartPosition = i + 1;
                        elementNameStartPosition = i + 1;

                        if (attributeName != null)
                        {
                            attributeValue = new string(firstChar, attributeValueStartPosition, i - attributeValueStartPosition).Trim('"');
                            currentProtoMessage._attributes[attributeName].Add(attributeValue);
                        }

                        attributeName = null;
                        elementName = null;
                    }

                    if (string4Chars.char1 == ':' && attributeName == null)
                    {
                        attributeName = new string(firstChar, attributeNameStartPosition, i - attributeNameStartPosition).Trim();
                        List<string> attributeList;
                        if (!currentProtoMessage._attributes.TryGetValue(attributeName, out attributeList))
                        {
                            attributeList = new List<string>();
                            currentProtoMessage._attributes.Add(attributeName, attributeList);
                        }

                        attributeValueStartPosition = i + 2;
                    }


                    //+++++++++++++++++++++ char2
                    if (string4Chars.char2 == '{')
                    {
                        elementName = new string(firstChar, elementNameStartPosition, i - elementNameStartPosition).Trim();
                        List<ProtoMessage3> submessagesList;
                        if (!currentProtoMessage._subMessages.TryGetValue(elementName, out submessagesList))
                        {
                            submessagesList = new List<ProtoMessage3>();
                            currentProtoMessage._subMessages.Add(elementName, submessagesList);
                        }
                        messagesStack.Push(currentProtoMessage);
                        currentProtoMessage = new ProtoMessage3();
                        submessagesList.Add(currentProtoMessage);
                    }

                    if (string4Chars.char2 == '}')
                    {
                        currentProtoMessage = messagesStack.Pop();
                    }

                    if (string4Chars.char2 == '\n')
                    {
                        attributeNameStartPosition = i + 2;
                        elementNameStartPosition = i + 2;

                        if (attributeName != null)
                        {
                            attributeValue = new string(firstChar, attributeValueStartPosition, i + 1 - attributeValueStartPosition).Trim('"');
                            currentProtoMessage._attributes[attributeName].Add(attributeValue);
                        }

                        attributeName = null;
                        elementName = null;
                    }

                    if (string4Chars.char2 == ':' && attributeName == null)
                    {
                        attributeName = new string(firstChar, attributeNameStartPosition, i + 1 - attributeNameStartPosition).Trim();
                        List<string> attributeList;
                        if (!currentProtoMessage._attributes.TryGetValue(attributeName, out attributeList))
                        {
                            attributeList = new List<string>();
                            currentProtoMessage._attributes.Add(attributeName, attributeList);
                        }

                        attributeValueStartPosition = i + 3;
                    }

                    //+++++++++++++++++++++ char3
                    if (string4Chars.char3 == '{')
                    {
                        elementName = new string(firstChar, elementNameStartPosition, i + 1 - elementNameStartPosition).Trim();
                        List<ProtoMessage3> submessagesList;
                        if (!currentProtoMessage._subMessages.TryGetValue(elementName, out submessagesList))
                        {
                            submessagesList = new List<ProtoMessage3>();
                            currentProtoMessage._subMessages.Add(elementName, submessagesList);
                        }
                        messagesStack.Push(currentProtoMessage);
                        currentProtoMessage = new ProtoMessage3();
                        submessagesList.Add(currentProtoMessage);
                    }

                    if (string4Chars.char3 == '}')
                    {
                        currentProtoMessage = messagesStack.Pop();
                    }

                    if (string4Chars.char3 == '\n')
                    {
                        attributeNameStartPosition = i + 3;
                        elementNameStartPosition = i + 3;

                        if (attributeName != null)
                        {
                            attributeValue = new string(firstChar, attributeValueStartPosition, i + 2 - attributeValueStartPosition).Trim('"');
                            currentProtoMessage._attributes[attributeName].Add(attributeValue);
                        }

                        attributeName = null;
                        elementName = null;
                    }

                    if (string4Chars.char3 == ':' && attributeName == null)
                    {
                        attributeName = new string(firstChar, attributeNameStartPosition, i + 2 - attributeNameStartPosition).Trim();
                        List<string> attributeList;
                        if (!currentProtoMessage._attributes.TryGetValue(attributeName, out attributeList))
                        {
                            attributeList = new List<string>();
                            currentProtoMessage._attributes.Add(attributeName, attributeList);
                        }

                        attributeValueStartPosition = i + 4;
                    }

                    //+++++++++++++++++++++ char4
                    if (string4Chars.char4 == '{')
                    {
                        elementName = new string(firstChar, elementNameStartPosition, i + 2 - elementNameStartPosition).Trim();
                        List<ProtoMessage3> submessagesList;
                        if (!currentProtoMessage._subMessages.TryGetValue(elementName, out submessagesList))
                        {
                            submessagesList = new List<ProtoMessage3>();
                            currentProtoMessage._subMessages.Add(elementName, submessagesList);
                        }
                        messagesStack.Push(currentProtoMessage);
                        currentProtoMessage = new ProtoMessage3();
                        submessagesList.Add(currentProtoMessage);
                    }

                    if (string4Chars.char4 == '}')
                    {
                        currentProtoMessage = messagesStack.Pop();
                    }

                    if (string4Chars.char4 == '\n')
                    {
                        attributeNameStartPosition = i + 4;
                        elementNameStartPosition = i + 4;

                        if (attributeName != null)
                        {
                            attributeValue = new string(firstChar, attributeValueStartPosition, i + 3 - attributeValueStartPosition).Trim('"');
                            currentProtoMessage._attributes[attributeName].Add(attributeValue);
                        }

                        attributeName = null;
                        elementName = null;
                    }

                    if (string4Chars.char4 == ':' && attributeName == null)
                    {
                        attributeName = new string(firstChar, attributeNameStartPosition, i + 3 - attributeNameStartPosition).Trim();
                        List<string> attributeList;
                        if (!currentProtoMessage._attributes.TryGetValue(attributeName, out attributeList))
                        {
                            attributeList = new List<string>();
                            currentProtoMessage._attributes.Add(attributeName, attributeList);
                        }

                        attributeValueStartPosition = i + 5;
                    }
                }

                // only 3 chars remain to parse;
                for (; i < _protoAsCharArray.Length; i++)
                {
                    char c = _protoAsCharArray[i];
                    if (c == '{')
                    {
                        elementName = new string(firstChar, elementNameStartPosition, i - elementNameStartPosition).Trim();
                        List<ProtoMessage3> submessagesList;
                        if (!currentProtoMessage._subMessages.TryGetValue(elementName, out submessagesList))
                        {
                            submessagesList = new List<ProtoMessage3>();
                            currentProtoMessage._subMessages.Add(elementName, submessagesList);
                        }
                        messagesStack.Push(currentProtoMessage);
                        currentProtoMessage = new ProtoMessage3();
                        submessagesList.Add(currentProtoMessage);
                    }

                    if (c == '}')
                    {
                        currentProtoMessage = messagesStack.Pop();
                    }

                    if (c == '\n')
                    {
                        attributeNameStartPosition = i + 1;
                        elementNameStartPosition = i + 1;

                        if (attributeName != null)
                        {
                            attributeValue = new string(firstChar, attributeValueStartPosition, i - attributeValueStartPosition).Trim('"');
                            currentProtoMessage._attributes[attributeName].Add(attributeValue);
                        }

                        attributeName = null;
                        elementName = null;
                    }

                    if (c == ':' && attributeName == null)
                    {
                        attributeName = new string(firstChar, attributeNameStartPosition, i - attributeNameStartPosition).Trim();
                        List<string> attributeList;
                        if (!currentProtoMessage._attributes.TryGetValue(attributeName, out attributeList))
                        {
                            attributeList = new List<string>();
                            currentProtoMessage._attributes.Add(attributeName, attributeList);
                        }

                        attributeValueStartPosition = i + 2;
                    }
                }
            }


        }

        public List<ProtoMessage3> GetElementList(string name)
        {
            return _subMessages.ContainsKey(name) ? _subMessages[name] : new List<ProtoMessage3>();
        }

        public ProtoMessage3 GetElement(string name)
        {
            return _subMessages.ContainsKey(name) && _subMessages[name].Count > 0 ? _subMessages[name][0] : null;
        }

        public List<string> GetAttributeList(string name)
        {
            return _attributes.ContainsKey(name) ? _attributes[name] : new List<string>();
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
            return _attributes.ContainsKey(name) && _attributes[name].Count > 0 ? _attributes[name][0] : null;
        }

        public List<string> GetKeys()
        {
            var res = new List<string>();

            void GetSubMessages(Fields<ProtoMessage3> messages)
            {
                foreach (KeyValuePair<string, List<ProtoMessage3>> msg in messages)
                {
                    foreach (ProtoMessage3 m in msg.Value)
                    {
                        res.Add(msg.Key);
                        GetSubMessages(m._subMessages);
                    }
                }
            }

            GetSubMessages(_subMessages);

            return res;
        }
    }
}