using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using ProtoMessage;
using NUnit.Framework;

namespace ProtoMessageBench
{
    internal static class Bench
    {
        private static void JustParse<T>(uint iterations, string protoAsText) where T : IProtoMessage<T>, new()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (uint i = 0; i < iterations; i++)
            {
                var protoMessage = new T();
                protoMessage.Parse(protoAsText);
            }

            stopwatch.Stop();
            Console.WriteLine($"{iterations} iterations JustParse for {typeof(T)} " +
                              $"finished in {stopwatch.Elapsed.TotalSeconds} sec");
        }

        private static void ReadAll<T>(uint iterations, string protoAsText) where T : IProtoMessage<T>, new()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (uint i = 0; i < iterations; i++)
            {
                var protoMessage = new T();
                protoMessage.Parse(protoAsText);

                List<string> keys = protoMessage.GetKeys();
                T lvlZeroMsg = protoMessage.GetElement("type1_message_level_zero");
                T lvlOneMsg = lvlZeroMsg.GetElement("message_level_one");
                T lvlTwoMsg = lvlOneMsg.GetElement("message_level_two");
                List<T> lvlThreeMessages = lvlTwoMsg.GetElementList("message_level_three");
                List<string> zeroIds = lvlZeroMsg.GetAttributeList("id");
                string uselessId = lvlTwoMsg.GetAttribute("useless_id");
                string longString = lvlTwoMsg.GetAttribute("long_string_attribute");
                lvlTwoMsg.GetAttribute("double_attribute");
                lvlTwoMsg.GetAttribute("integer_value");
                lvlTwoMsg.GetAttribute("description");
                lvlTwoMsg.GetAttribute("why");
                lvlTwoMsg.GetAttribute("another_double");
                lvlTwoMsg.GetAttribute("and_one_more_string");
                lvlTwoMsg.GetAttribute("its_boolean");
                lvlTwoMsg.GetAttributeList("we_need_a_repeated_string_too");
                lvlTwoMsg.GetAttribute("and_here_too");
                lvlTwoMsg.GetAttribute("help_me");
                lvlTwoMsg.GetAttribute("i_do_not_know");
                lvlTwoMsg.GetAttribute("what_to");
                lvlTwoMsg.GetAttribute("write_here");
                string lvlOneBool = lvlOneMsg.GetAttribute("and_boolean_in_the_end");
                string? needToWriteAttr = null;

                foreach (T msg in lvlThreeMessages)
                {
                    needToWriteAttr = msg.GetAttribute("need_to_write_something_here");
                    msg.GetAttribute("why_all_zeros");
                }

                lvlTwoMsg.GetAttribute("master_yoda_said");
                T uniqueLvl3 = lvlTwoMsg.GetElement("message_level_three_unique");
                uniqueLvl3.GetAttribute("need_to_write_something_here");
                uniqueLvl3.GetAttribute("why_all_zeros");
                T uniqueOtherL3 = lvlTwoMsg.GetElement("message_level_three_but_unique");
                string key = uniqueOtherL3.GetAttribute("key");
                string text = uniqueOtherL3.GetAttribute("text");

                // made a check once per 1000 iterations just to make sure everything is correct
                if (i % 1000 != 0)
                {
                    continue;
                }

                Assert.AreEqual(13, keys.Count);
                Assert.IsNotEmpty(lvlThreeMessages);
                Assert.AreEqual(28, zeroIds.Count);
                Assert.AreEqual("666", zeroIds[20]);
                Assert.AreEqual("254", uselessId);
                Assert.AreEqual("2387o497ghl233j rl dj nf we \"f\"jp w38f p!@#$ @#$% " +
                                "@!~@!#$@Ss42``42344 2 34234p909k09fjd", longString);
                Assert.AreEqual("false", lvlOneBool);
                Assert.AreEqual("1", needToWriteAttr);
                Assert.AreEqual("1234554321", key);
                Assert.AreEqual("ha ha", text);
            }

            stopwatch.Stop();
            Thread.Sleep(100); // TODO: fix. Console in Rider goes mad without this
            Console.WriteLine($"{iterations} iterations ReadAll for {typeof(T)} " +
                              $"finished in {stopwatch.Elapsed.TotalSeconds} sec");
            Thread.Sleep(100);
        }

        private static void PartialRead<T>(uint iterations, string protoAsText) where T : IProtoMessage<T>, new()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (uint i = 0; i < iterations; i++)
            {
                var protoMessage = new T();
                protoMessage.Parse(protoAsText);

                T msgLevelZero = protoMessage.GetElement("type1_message_level_zero");
                string intId = msgLevelZero.GetAttribute("top_level_int");
                T msgLevelTwo = msgLevelZero.GetElement("message_level_one").GetElement("message_level_two");
                string uselessId = msgLevelTwo.GetAttribute("useless_id");
                string longString = msgLevelTwo.GetAttribute("long_string_attribute");

                // made a check once per 1000 iterations just to make sure everything is correct
                if (i % 1000 != 0)
                {
                    continue;
                }

                Assert.AreEqual("0", intId);
                Assert.AreEqual("254", uselessId);
                Assert.AreEqual("2387o497ghl233j rl dj nf we \"f\"jp w3" +
                                "8f p!@#$ @#$% @!~@!#$@Ss42``42344 2 34234p909k09fjd", longString);
            }

            stopwatch.Stop();
            Console.WriteLine($"{iterations} iterations PartialRead for {typeof(T)} " +
                              $"finished in {stopwatch.Elapsed.TotalSeconds} sec");
        }

        public static void Main(string[] args)
        {
            Stream dataStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("ProtoMessageBench.TestData.txt");
            var dataReader = new StreamReader(dataStream ?? throw new Exception("Cannot read data"));
            string testData = dataReader.ReadToEnd();

            switch (args.Length > 1 ? args[1] : "")
            {
                case "ProtoMessageOld":
                    JustParse<ProtoMessageOld>(uint.Parse(args[0]), testData);
                    PartialRead<ProtoMessageOld>(uint.Parse(args[0]), testData);
                    ReadAll<ProtoMessageOld>(uint.Parse(args[0]), testData);
                    break;
                case "ProtoMessage":
                    JustParse<ProtoMessage.ProtoMessage>(uint.Parse(args[0]), testData);
                    PartialRead<ProtoMessage.ProtoMessage>(uint.Parse(args[0]), testData);
                    ReadAll<ProtoMessage.ProtoMessage>(uint.Parse(args[0]), testData);
                    break;
                case "ProtoMessage4":
                    JustParse<ProtoMessage4>(uint.Parse(args[0]), testData);
                    PartialRead<ProtoMessage4>(uint.Parse(args[0]), testData);
                    ReadAll<ProtoMessage4>(uint.Parse(args[0]), testData);
                    break;
                default:
                    throw new Exception("Provide ProtoMessage number - 0, 2 or 4 in second argument");
            }
        }
    }
}