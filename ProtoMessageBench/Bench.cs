using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ProtoMessageOriginal;
using NUnit.Framework;

namespace ProtoMessageBench
{
    internal static class Bench
    {

        private static void JustParse<T>(uint iterations, string protoAsText) where T : IProtoMessage<T>, new()
        {
            var stopwatch = new Stopwatch();
            Console.Write($"{iterations} iterations JustParse for {typeof(T)} - ");
            stopwatch.Start();
            
            for (uint i = 0; i < iterations; i++)
            {
                var protoMessage = new T();
                protoMessage.Parse(protoAsText);
            }
            
            stopwatch.Stop();
            Console.WriteLine($"finished in {stopwatch.Elapsed.TotalSeconds} sec");
        }
        
        private static void ReadAll<T>(uint iterations, string protoAsText) where T : IProtoMessage<T>, new()
        {
            var stopwatch = new Stopwatch();
            Console.Write($"{iterations} iterations JustParse for {typeof(T)} - ");
            stopwatch.Start();

            for (uint i = 0; i < iterations; i++)
            {
                var protoMessage = new T();
                protoMessage.Parse(protoAsText);

                List<string> keys = protoMessage.GetKeys();
                
                // made a check once per 1000 iterations just to make sure everything is correct
                if (i % 1000 != 0)
                {
                    continue;
                }
                Assert.AreEqual(13, keys.Count);
            }

            stopwatch.Stop();
            Console.WriteLine($"finished in {stopwatch.Elapsed.TotalSeconds} sec");
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