using System.Collections.Generic;
using NUnit.Framework;
using ProtoMessage;

namespace ProtoMessageUT
{
    [TestFixture]
    internal class ProtoBuffUt
    {
        private const string RootMessage = "root_message";
        private const string RootMessage2 = "root_message2";
        private const string RepeatedSubMessage = "repeated_sub_message";
        private const string RepeatedAttribute = "repeated_sub_message";
        private const string NotExistingMessage = "not_existing_message";

        private static readonly (string, string) SomeText =
            ("jedi_phrases", "- These aren't the droids you're looking for!");

        private static readonly (string, string) SomeSingleNumber = ("favourite_number", "7");
        private static readonly (string, string) FirstInt = (RepeatedAttribute, "74373");
        private static readonly (string, string) SecondInt = (RepeatedAttribute, "2341");

        private static readonly (string, string) RootAttribute1 = ("root_attr_1", "root_attr_1_val");
        private static readonly (string, string) RootAttribute2 = ("root_attr_2", "21342345");
        private static readonly (string, string) RootAttributeLast1 = ("root_attr_last_1", "21342345");
        private static readonly (string, string) RootAttributeLast2 = ("root_attr_last_2", "last_sting_attr");

        private static readonly (string, string) ExtensionAttribute1 = ("[Extension_1.extension_attribute]",
            "extension attribute value");

        private static readonly (string, string) ExtensionAttribute2 = ("[Extension_1.extension_attribute2]",
            "extension attribute value 2");

        private static readonly (string, string) RootMsg2Attr = ("dirty_string",
            " ha-ha TAKE THIS: \"quoted!\" and this: { ` } { `{ `{ }} hope your code died here ");

        private readonly string _protoText =
            $"{RootAttribute1.Item1}: \"{RootAttribute1.Item2}\"\n" +
            $"{RootAttribute2.Item1}: {RootAttribute2.Item2}\n" +
            $"{RootMessage} {{\n"
            + $"  {SomeSingleNumber.Item1}: {SomeSingleNumber.Item2}\n"
            + $"  {SomeText.Item1}: \"{SomeText.Item2}\"\n"
            + $"  {RepeatedSubMessage} {{\n"
            + "    type: 1\n"
            + "    time: 638427648\n"
            + $"    {RepeatedAttribute}: {FirstInt.Item2}\n"
            + $"    {RepeatedAttribute}: {SecondInt.Item2}\n"
            + "  }\n"
            + $"  {RepeatedSubMessage} {{\n"
            + "    type: 2\n"
            + $"    {RepeatedAttribute}: {FirstInt.Item2}\n"
            + $"    {RepeatedAttribute}: {SecondInt.Item2}\n"
            + "  }\n"
            + "}\n" +
            $"{RootMessage2} {{\n"
            + $"  {RootMsg2Attr.Item1}: \"{RootMsg2Attr.Item2}\"\n"
            + $"  {ExtensionAttribute2.Item1}: \"{ExtensionAttribute2.Item2}\"\n"
            + "}\n"
            + $"{RootAttributeLast1.Item1}: {RootAttributeLast1.Item2}\n"
            + $"{ExtensionAttribute1.Item1}: \"{ExtensionAttribute1.Item2}\"\n"
            + $"{RootAttributeLast2.Item1}: \"{RootAttributeLast2.Item2}\"\n";
        
        private void ProtoParse<T>() where T : IProtoMessage<T>, new()
        {
            var pm = new T();
            pm.Parse(_protoText);
            Assert.AreEqual(RootAttribute1.Item2, pm.GetAttribute(RootAttribute1.Item1));
            Assert.AreEqual(RootAttribute2.Item2, pm.GetAttribute(RootAttribute2.Item1));
            Assert.AreEqual(RootAttributeLast1.Item2, pm.GetAttribute(RootAttributeLast1.Item1));
            Assert.AreEqual(RootAttributeLast2.Item2, pm.GetAttribute(RootAttributeLast2.Item1));
            Assert.AreEqual(ExtensionAttribute1.Item2, pm.GetAttribute(ExtensionAttribute1.Item1));

            Assert.AreEqual(new List<T>(), pm.GetElementList(NotExistingMessage));
            Assert.AreEqual(new List<string>(), pm.GetAttributeList(NotExistingMessage));

            T rootMsg = pm.GetElement(RootMessage);
            Assert.IsNotNull(rootMsg);


            Assert.AreEqual(2, pm.Keys.Count);
            Assert.Contains(RootMessage, pm.Keys);
            Assert.Contains(RootMessage2, pm.Keys);

            Assert.AreEqual(SomeSingleNumber.Item2, rootMsg.GetAttribute(SomeSingleNumber.Item1));
            Assert.AreEqual(SomeText.Item2, rootMsg.GetAttribute(SomeText.Item1));

            List<T> listPm = rootMsg.GetElementList(RepeatedSubMessage);
            Assert.That(listPm.Count, Is.EqualTo(2));
            foreach (T repeatedSubMsg in listPm)
            {
                List<string> attrList = repeatedSubMsg.GetAttributeList(RepeatedAttribute);
                Assert.AreEqual(FirstInt.Item2, repeatedSubMsg.GetAttribute(RepeatedAttribute));
                Assert.AreEqual(2, attrList.Count);
                Assert.AreEqual(SecondInt.Item2, attrList[1]);
            }

            Assert.AreEqual(2, rootMsg.Keys.Count);

            T rootMsg2 = pm.GetElement(RootMessage2);
            Assert.AreEqual(RootMsg2Attr.Item2, rootMsg2.GetAttribute(RootMsg2Attr.Item1));
            Assert.AreEqual(ExtensionAttribute2.Item2, rootMsg2.GetAttribute(ExtensionAttribute2.Item1));

            Assert.AreEqual(true, pm.HasKey(RootMessage));
            Assert.AreEqual(true, pm.HasKey(RootMessage2));
            Assert.AreEqual(true, rootMsg.HasKey(RepeatedSubMessage));

            Assert.AreEqual(false, pm.HasKey(RepeatedSubMessage));
            Assert.AreEqual(false, pm.HasKey(RootAttribute1.Item1));
            Assert.AreEqual(false, pm.HasKey(NotExistingMessage));
            Assert.AreEqual(false, rootMsg.HasKey(NotExistingMessage));
            Assert.AreEqual(false, rootMsg.HasKey(RootMessage));
        }
        
        [Test]
        public void ProtoMessage()
        {
            ProtoParse<ProtoMessage.ProtoMessage>();
        }

        [Test]
        public void ProtoMessage4()
        {
            ProtoParse<ProtoMessage4>();
        }
    }
}