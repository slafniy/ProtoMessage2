using System.Collections.Generic;

namespace ProtoMessage
{
    public interface IProtoMessage<TType> where TType : IProtoMessage<TType>, new()
    {
        List<TType> GetElementList(string name);  // to get repeated messages
        TType GetElement(string name);  // to get optional/required message or first message from repeated
        List<string> GetAttributeList(string name);  // to get repeated attribute values
        T GetAttribute<T>(string name) where T : struct;  
        T? GetAttributeOrNull<T>(string name) where T : struct;
        string GetAttribute(string name);  // to get attribute value
        void Parse(string text);
        bool HasKey(string name);
        List<string> Keys { get; }  // return inner message names (without recursion!)
    }
}