using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoMessageOriginal;

namespace ProtoMessageUT
{
    [TestFixture]
    internal class ProtoBuffUt
    {
        private const string RootMessage = "root_message";
        private const string RootMessage2 = "root_message2";
        private const string RepeatedSubmessage = "repeated_sub_message";
        private const string RepeatedAttribute = "repeated_sub_message";
        private const string NotExistingMessage = "not_existing_message";

        private static readonly (string, string) SomeText = 
            ("jedi_phrases", "- These aren`t the droids you're looking for!");

        private static readonly (string, string) SomeSingleNumber = ("favourite_number", "7");
        private static readonly (string, string) FirstInt = (RepeatedAttribute, "74373");
        private static readonly (string, string) SecondInt = (RepeatedAttribute, "2341");

        private static readonly (string, string) RootAttribute1 = ("root_attr_1", "root_attr_1_val");
        private static readonly (string, string) RootAttribute2 = ("root_attr_2", "21342345");
        
        private static readonly (string, string) RootMsg2Attr = ("dirty_string", " ha-ha TAKE THIS: \"quoted!\" ! ");

        private readonly string _protoText =
            $"{RootAttribute1.Item1}: {RootAttribute1.Item2}\n" +
            $"{RootAttribute2.Item1}: {RootAttribute2.Item2}\n" +
            $"{RootMessage} {{\n"
            + $"  {SomeSingleNumber.Item1}: {SomeSingleNumber.Item2}\n"
            + $"  {SomeText.Item1}: \"{SomeText.Item2}\"\n"
            + $"  {RepeatedSubmessage} {{\n"
            + "    type: 1\n"
            + "    time: 638427648\n"
            + $"    {RepeatedAttribute}: {FirstInt.Item2}\n"
            + $"    {RepeatedAttribute}: {SecondInt.Item2}\n"
            + "  }\n"
            + $"  {RepeatedSubmessage} {{\n"
            + "    type: 2\n"
            + $"    {RepeatedAttribute}: {FirstInt.Item2}\n"
            + $"    {RepeatedAttribute}: {SecondInt.Item2}\n"
            + "  }\n"
            + "}\n" +
            $"{RootMessage2} {{\n"
            + $"  {RootMsg2Attr.Item1}: {RootMsg2Attr.Item2}\n"
            + "}\n";

        private void ProtoParse<T>() where T : IProtoMessage<T>, new()
        {
            var pm = new T();
            pm.Parse(_protoText);

            Assert.AreEqual(new List<T>(), pm.GetElementList(NotExistingMessage));
            Assert.AreEqual(new List<string>(), pm.GetAttributeList(NotExistingMessage));

            T rootMsg = pm.GetElement(RootMessage);
            Assert.IsNotNull(rootMsg);
            Assert.Contains(RootMessage, pm.GetKeys());
            Assert.Contains(RootMessage2, pm.GetKeys());
            Assert.AreEqual(4, pm.GetKeys().Count);
            Assert.AreEqual(2, pm.GetKeys().Select(x => x).Count(x => x == RepeatedSubmessage));
            
            Assert.AreEqual(SomeSingleNumber.Item2, rootMsg.GetAttribute(SomeSingleNumber.Item1));
            Assert.AreEqual(SomeText.Item2, rootMsg.GetAttribute(SomeText.Item1));

            List<T> listPm = rootMsg.GetElementList(RepeatedSubmessage);
            Assert.That(listPm.Count, Is.EqualTo(2));
            foreach (T repeatedSubMsg in listPm)
            {
                List<string> attrList = repeatedSubMsg.GetAttributeList(RepeatedAttribute);
                Assert.AreEqual(FirstInt.Item2, repeatedSubMsg.GetAttribute(RepeatedAttribute));
                Assert.AreEqual(2, attrList.Count);
                Assert.AreEqual(SecondInt.Item2, attrList[1]);
            }
            
            Assert.AreEqual(2, rootMsg.GetKeys().Select(x => x).Count(x => x == RepeatedSubmessage));
            Assert.AreEqual(2, rootMsg.GetKeys().Count);

            T rootMsg2 = pm.GetElement(RootMessage2);
            Assert.AreEqual(RootMsg2Attr.Item2, rootMsg2.GetAttribute(RootMsg2Attr.Item1));
        }

        [Test]
        public void ProtoParse()
        {
            ProtoParse<ProtoMessage>();
            ProtoParse<ProtoMessage2>();
        }
    }
}