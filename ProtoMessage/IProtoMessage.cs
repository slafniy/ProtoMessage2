using System.Collections.Generic;

namespace ProtoMessage
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
        bool HasKey(string name);
        List<string> Keys { get; }
    }
}