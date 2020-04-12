using System.Collections.Generic;
using NUnit.Framework;
using ProtoMessageOriginal;

namespace ProtoMessageUT
{
    [TestFixture]
    internal class ProtoBuffUt
    {
        private const string RootMessage = "root_message";
        
        private const string SomeText = "jedi_phrases";
        private const string SomeTextVal = "- These aren`t the droids you're looking for!";

        private const string SomeSingleNumber = "favourite_number";
        private const string SomeSingleNumberVal = "7";

        private const string FirstScaledPrice = "5883";

        private const string SecondScaledPrice = "14";
        
        private const string RepeatedSubmessage = "repeated_sub_message";

        

        private readonly string _protoText =
            $"{RootMessage} {{\n"
            + $"  {SomeSingleNumber}: {SomeSingleNumberVal}\n"
            + $"  {SomeText}: \"{SomeTextVal}\"\n"
            + $"  {RepeatedSubmessage} {{\n"
            + "    type: 1\n"
            + "    quote_utc_time: 638427648\n"
            + $"    scaled_price: {FirstScaledPrice}\n"
            + $"    scaled_price: {SecondScaledPrice}\n"
            + "  }\n"
            + $"  {RepeatedSubmessage} {{\n"
            + "    type: 2\n"
            + $"    scaled_price: {FirstScaledPrice}\n"
            + $"    scaled_price: {SecondScaledPrice}\n"
            + "  }\n"
            + "}";

        private void ProtoParse<T>() where T : IProtoMessage<T>, new()
        {
            var pm = new T();
            pm.Parse(_protoText);
            pm = pm.GetElement(RootMessage);
            List<T> listPm = pm.GetElementList(RepeatedSubmessage);
            Assert.IsNotNull(pm);
            
            Assert.AreEqual(SomeSingleNumberVal, pm.GetAttribute(SomeSingleNumber));
            Assert.AreEqual(SomeTextVal, pm.GetAttribute(SomeText));
            
            Assert.That(listPm.Count, Is.EqualTo(2));
            foreach (T protoMessage in listPm)
            {
                List<string> attrList = protoMessage.GetAttributeList("scaled_price");
                Assert.That(protoMessage.GetAttribute("scaled_price"), Is.EqualTo(FirstScaledPrice));
                Assert.That(attrList.Count, Is.EqualTo(2));
                Assert.That(attrList[1], Is.EqualTo(SecondScaledPrice));
            }

            Assert.AreEqual(new List<string> {RepeatedSubmessage, RepeatedSubmessage}, pm.GetKeys());
        }

        [Test]
        public void ProtoParse()
        {
            ProtoParse<ProtoMessage>();
            ProtoParse<ProtoMessage2>();
        }
    }
}