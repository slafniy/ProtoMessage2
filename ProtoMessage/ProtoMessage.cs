using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ProtoMessage
{
    // Chars bitwise negatives - bitwise AND is faster than chars comparison
    public static class NChars
    {
        public const int Space = ~' ';
        public const int NewLine = ~'\n';
        public const int Quote = ~'"';
    }

    public struct Attribute
    {
        private readonly int _index; // global position of ':' in text
        public string Value => _value ?? ParseAttributeValue();
        private string _value;
        private readonly string _protoAsText;

        public Attribute(int index, string protoAsText)
        {
            _index = index;
            _value = null;
            _protoAsText = protoAsText;
        }

        public bool CheckName(string name)
        {
            return ProtoMessage.CheckName(name, _index, 1 /* skip colon */, _protoAsText);
        }

        private string ParseAttributeValue()
        {
            int index = _index;
            int start = index + 2; // skip whitespace
            while (index < _protoAsText.Length && (_protoAsText[index] & NChars.NewLine) != 0)
            {
                index++;
            }

            // This works faster than .Trim()
            if ((_protoAsText[start] & NChars.Quote) == 0)
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
        public readonly List<int> ChildIndexes; // sub-message indexes in sub-messages matrix
        public readonly List<int> AttributeIndexes; // attribute indexes in attributes matrix
        public string Name => _name ?? ParseName();
        private string _name;
        private readonly string _protoAsText;

        public SubMessage(int index, string protoAsText)
        {
            _index = index;
            ChildIndexes = new List<int>();
            AttributeIndexes = new List<int>();
            _name = null;
            _protoAsText = protoAsText;
        }

        // For debug purposes
        public override string ToString()
        {
            return $"Index: {_index} ChildIndexes: {string.Join(", ", ChildIndexes)} " +
                   $"AttributeIndexes: {string.Join(", ", AttributeIndexes)}";
        }

        public bool CheckName(string name)
        {
            return ProtoMessage.CheckName(name, _index, 2 /* skip '{' and whitespace */, _protoAsText);
        }

        private string ParseName()
        {
            int idx = _index;
            // Look backward for newline or message beginning
            int start = idx -= 1; // skip whitespace for message or colon for attribute
            while (start > 0 && (_protoAsText[start - 1] & NChars.Space) != 0 &&
                   (_protoAsText[start - 1] & NChars.NewLine) != 0)
            {
                start--;
            }

            _name = _protoAsText.Substring(start, idx - start);
            return _name;
        }
    }

    public class ProtoMessage : IProtoMessage<ProtoMessage>
    {
        // These fields are same for every sub-ProtoMessage
        private readonly List<SubMessage> _subMessagesMatrix = new List<SubMessage>(); // all levels sub-messages
        private Attribute[] _attributesMatrix; // all levels attributes
        private string _protoAsText; // original input string

        private readonly int _indexInMatrix; // place of this instance in global _subMessagesMatrix

        private List<string> _keys; // to cache result of Keys property

        // Called only to return sub-message TODO: maybe it would be better to have xPath-like access?
        private ProtoMessage(List<SubMessage> matrix, ref Attribute[] attrMatrix, string protoAsText, int indexInMatrix)
        {
            _subMessagesMatrix = matrix;
            _attributesMatrix = attrMatrix;
            _protoAsText = protoAsText;
            _indexInMatrix = indexInMatrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // saves about 2.8% parsing time
        public static bool CheckName(string name, int initialPosition, int howManySkip, string protoAsText)
        {
            int nameIdx = name.Length - 1; // the end of the name
            initialPosition -= howManySkip; // e.g. skip '{' and whitespace for message or colon for attribute

            // Look backward for newline or message beginning
            while (nameIdx >= 0)
            {
                if (name[nameIdx] != protoAsText[initialPosition])
                {
                    return false;
                }

                initialPosition--;
                nameIdx--;
            }

            // Do not save the result! Substring is too slow!
            return initialPosition < 0 || (protoAsText[initialPosition] & NChars.Space) == 0 ||
                   (protoAsText[initialPosition] & NChars.NewLine) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessColon(ref int currentAttributeIndex, int i, Stack<int> currentParentMessages, 
            ref bool isPreviousColon)
        {
            _attributesMatrix[currentAttributeIndex] = new Attribute(i, _protoAsText);
            _subMessagesMatrix[currentParentMessages.Peek()].AttributeIndexes.Add(currentAttributeIndex);
            currentAttributeIndex++;
            isPreviousColon = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessOpenBrace(int i, Stack<int> currentParentMessages)
        {
            _subMessagesMatrix.Add(new SubMessage(i, _protoAsText));
            _subMessagesMatrix[currentParentMessages.Peek()].ChildIndexes.Add(_subMessagesMatrix.Count - 1);
            currentParentMessages.Push(_subMessagesMatrix.Count - 1);
        }
        

        // Should be called once after instance creation
        public void Parse(string protoAsText)
        {
            _protoAsText = protoAsText;
            _attributesMatrix = new Attribute[_protoAsText.Length / 5];  // minimal possible attribute has 5 chars
            _subMessagesMatrix.Add(new SubMessage(0, protoAsText)); // root message that contains 0-level messages

            int currentAttributeIndex = 0;
            bool isPreviousColon = false; // to process colons inside attribute values

            var currentParentMessages = new Stack<int>(); // temporary saves parent message indexes
            currentParentMessages.Push(0);

            // iterate the input text only once and save all control symbol positions
            for (int i = 0; i < _protoAsText.Length; i++)
            {
                switch (isPreviousColon)
                {
                    case false:
                        switch (_protoAsText[i])
                        {
                            case ':':
                                ProcessColon(ref currentAttributeIndex, i, currentParentMessages, ref isPreviousColon);
                                break;
                            case '{':
                                ProcessOpenBrace(i, currentParentMessages);
                                break;
                            case '}':
                                currentParentMessages.Pop();
                                break;
                        }
                        break;
                    case true:
                        isPreviousColon = _protoAsText[i] != '\n';
                        break;
                }
            }
        }

        public ProtoMessage()
        {
        }

        public List<ProtoMessage> GetElementList(string name)
        {
            var res = new List<ProtoMessage>();
            foreach (int idx in _subMessagesMatrix[_indexInMatrix].ChildIndexes)
            {
                if (_subMessagesMatrix[idx].CheckName(name))
                {
                    res.Add(new ProtoMessage(_subMessagesMatrix, ref _attributesMatrix, _protoAsText, idx));
                }
            }

            return res;
        }

        public ProtoMessage GetElement(string name)
        {
            foreach (int idx in _subMessagesMatrix[_indexInMatrix].ChildIndexes)
            {
                if (_subMessagesMatrix[idx].CheckName(name))
                {
                    return new ProtoMessage(_subMessagesMatrix, ref _attributesMatrix, _protoAsText, idx);
                }
            }

            return null;
        }

        public List<string> GetAttributeList(string name)
        {
            var res = new List<string>();
            foreach (int i in _subMessagesMatrix[_indexInMatrix].AttributeIndexes)
            {
                if (_attributesMatrix[i].CheckName(name))
                {
                    res.Add(_attributesMatrix[i].Value);
                }
            }

            return res;
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
            foreach (int idx in _subMessagesMatrix[_indexInMatrix].AttributeIndexes)
            {
                if (_attributesMatrix[idx].CheckName(name))
                {
                    return _attributesMatrix[idx].Value;
                }
            }

            return null;
        }

        public bool HasKey(string subMessageName)
        {
            foreach (int childIndex in _subMessagesMatrix[_indexInMatrix].ChildIndexes)
            {
                if (_subMessagesMatrix[childIndex].CheckName(subMessageName))
                {
                    return true;
                }
            }

            return false;
        }

        // Returns sub-message names if any (NOT recursive!)
        public List<string> Keys
        {
            get
            {
                if (_keys != null)
                {
                    return _keys;
                }

                _keys = new List<string>();

                foreach (int childIndex in _subMessagesMatrix[_indexInMatrix].ChildIndexes)
                {
                    _keys.Add(_subMessagesMatrix[childIndex].Name);
                }

                return _keys;
            }
        }
    }
}