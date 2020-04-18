using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using ProtoMessageOriginal;
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
                
                // made a check once per 1000 iterations just to make sure everything is correct
                if (i % 1000 != 0)
                {
                    continue;
                }
                Assert.AreEqual(13, keys.Count);
                Assert.IsNotEmpty(lvlThreeMessages);
            }

            stopwatch.Stop();
            Thread.Sleep(500);  // TODO: fix. Console in Rider goes mad without this
            Console.WriteLine($"{iterations} iterations ReadAll for {typeof(T)} " +
                              $"finished in {stopwatch.Elapsed.TotalSeconds} sec");
            Thread.Sleep(500);
        }

        public static void Main(string[] args)
        {
            Stream dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoMessageBench.TestData.txt");
            var dataReader = new StreamReader(dataStream ?? throw new Exception("Cannot read data"));
            string testData = dataReader.ReadToEnd();

            JustParse<ProtoMessage>(uint.Parse(args[0]), testData);
            JustParse<ProtoMessage2>(uint.Parse(args[0]), testData);
            
            ReadAll<ProtoMessage>(uint.Parse(args[0]), testData);
            ReadAll<ProtoMessage2>(uint.Parse(args[0]), testData);
        }
    }
}