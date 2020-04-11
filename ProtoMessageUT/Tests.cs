using System.Collections.Generic;
using NUnit.Framework;
using ProtoMessageOriginal;

namespace ProtoMessageUT
{
    [TestFixture]
    internal class ProtoBuffUt
    {
        private const string ContractId = "45";
        private const string FirstScaledPrice = "5883";
        private const string SecondScaledPrice = "14";

        private readonly string _protoText =
            "real_time_market_data {\n"
            + $"  contract_id: {ContractId}\n"
            + "  quote {\n"
            + "    type: 1\n"
            + "    quote_utc_time: 638427648\n"
            + $"    scaled_price: {FirstScaledPrice}\n"
            + $"    scaled_price: {SecondScaledPrice}\n"
            + "  }\n"
            + "  quote {\n"
            + "    type: 2\n"
            + $"    scaled_price: {FirstScaledPrice}\n"
            + $"    scaled_price: {SecondScaledPrice}\n"
            + "  }\n"
            + "}";

        [Test]
        public void ProtoParse()
        {
            var pm = new ProtoMessage();
            pm.Parse(_protoText);
            pm = pm.GetElement("real_time_market_data");
            var listPm = pm.GetElementList("quote");
            Assert.IsNotNull(pm);
            Assert.That(pm.GetAttribute("contract_id"), Is.EqualTo(ContractId));
            Assert.That(listPm.Count, Is.EqualTo(2));
            foreach (var protoMessage in listPm)
            {
                var attrList = protoMessage.GetAttributeList("scaled_price");
                Assert.That(protoMessage.GetAttribute("scaled_price"), Is.EqualTo(FirstScaledPrice));
                Assert.That(attrList.Count, Is.EqualTo(2));
                Assert.That(attrList[1], Is.EqualTo(SecondScaledPrice));
            }

            Assert.AreEqual(new List<string> {"quote", "quote"}, pm.GetKeys());
        }
    }
}