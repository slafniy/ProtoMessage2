using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoMessageOriginal
{
    public class ProtoMessage2 : IProtoMessage<ProtoMessage2>
    {
        private int _startIdx;
        private int _endIdx;
        private string _protoAsText;
        private uint _level = 0;

        // Key: index of parent message, Value: index of attribute
        private readonly Dictionary<int, int> _attrIndexes = new Dictionary<int, int>();

        // Key: Message level, Value: message indexes list
        private readonly Dictionary<uint, List<int>> _msgIndexes = new Dictionary<uint, List<int>>();

        // Contains sub-messages for CURRENT level only
        private ILookup<string, KeyValuePair<string, ProtoMessage2>> _subMessages;

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

            FillCurrentLevelNames();
        }

        private void FillCurrentLevelNames()
        {
            List<int> currLvlIndexes = _msgIndexes[_level];
            var subMessagesList = new List<KeyValuePair<string, ProtoMessage2>>();
            foreach (int idx in currLvlIndexes)
            {
                subMessagesList.Add(new KeyValuePair<string, ProtoMessage2>(GetMessageName(idx),
                    new ProtoMessage2(_level + 1, _attrIndexes, _msgIndexes)));
            }

            _subMessages = subMessagesList.ToLookup(x => x.Key);
        }

        private string GetMessageName(int idx)
        {
            // Look backward for newline or message beginning
            int start = idx -= 1; // skip whitespace
            while (start > _startIdx && _protoAsText[start - 1] != ' ' && _protoAsText[start - 1] != '\n')
            {
                start--;
            }

            return _protoAsText.Substring(start, idx - start);
        }

        public ProtoMessage2()
        {
        }

        private ProtoMessage2(uint level, Dictionary<int, int> attrIndexes, Dictionary<uint, List<int>> msgIndexes)
        {
            _level = level;
            _attrIndexes = attrIndexes;
            _msgIndexes = msgIndexes;
            // FillCurrentLevelNames();
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