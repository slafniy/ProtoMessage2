using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoMessageOriginal
{
    public class ProtoMessage2 : IProtoMessage<ProtoMessage2>
    {
        private int _startIdx;
        private int _endIdx;
        private string _protoAsText;
        
        // Key: index of parent message, Value: index of attribute
        private readonly Dictionary<int, int> _attrIndexes = new Dictionary<int, int>();
        
        // Key: Message level, Value: message indexes list
        private readonly Dictionary<uint, List<int>> _msgIndexes = new Dictionary<uint, List<int>>();

        private void ParseBody()
        {
            var parentIndexes = new Stack<int>();
            parentIndexes.Push(-1); // -1 means no parent
            uint currentLevel = 0;
            for (int i = 0; i <= _endIdx; i++)
            {
                char c = _protoAsText[i];
                switch (c)
                {
                    case ':':
                        _attrIndexes[parentIndexes.Peek()] = i;
                        break;
                    case '{':
                        parentIndexes.Push(i);
                        if (!_msgIndexes.TryGetValue(currentLevel, out List<int> msgIndexesForLevel))
                        {
                            msgIndexesForLevel = new List<int>();
                            _msgIndexes[currentLevel] = msgIndexesForLevel;
                        }
                        msgIndexesForLevel.Add(i);
                        currentLevel++;
                        break;
                    case '}':
                        parentIndexes.Pop();
                        currentLevel--;
                        break;
                }
            }
        }

        public ProtoMessage2()
        {
        }

        public override string ToString()
        {
            return _protoAsText.Substring(_startIdx, _endIdx - _startIdx);
        }

        public List<ProtoMessage2> GetElementList(string name)
        {
            throw new System.NotImplementedException();
        }

        public ProtoMessage2 GetElement(string name)
        {
            throw new System.NotImplementedException();
        }

        public List<string> GetAttributeList(string name)
        {
            throw new System.NotImplementedException();
        }

        public T GetAttribute<T>(string name) where T : struct
        {
            throw new System.NotImplementedException();
        }

        public T? GetAttributeOrNull<T>(string name) where T : struct
        {
            throw new System.NotImplementedException();
        }

        public string GetAttribute(string name)
        {
            throw new System.NotImplementedException();
        }

        public void Parse(string text)
        {
            _protoAsText = text;
            _startIdx = 0;
            _endIdx = text.Length - 1;
            ParseBody();
        }

        public List<string> GetKeys()
        {
            throw new System.NotImplementedException();
        }
    }
}