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
        private static void DoBench<T>(uint iterationsLimit, string protoAsText) where T : IProtoMessage<T>, new()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (uint iteration = 0; iteration <= iterationsLimit; iteration++)
            {
                var pm = new T();
                pm.Parse(protoAsText);

                var topLvlKeys = pm.GetKeys();  // is it supposed for this, right?
                
                // try to parse some data from 3 lvl message
                var mainMsg = pm.GetElementList("type1_message_level_zero");
            }

            stopwatch.Stop();
            Console.WriteLine(
                $"{iterationsLimit} iterations finished for {typeof(T)} in {stopwatch.Elapsed.TotalSeconds} sec");
        }

        public static void Main(string[] args)
        {
            Stream dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoMessageBench.TestData.txt");
            var dataReader = new StreamReader(dataStream ?? throw new Exception("Cannot read data"));
            string testData = dataReader.ReadToEnd();

            // DoBench<ProtoMessage>(uint.Parse(args[0]), testData);
            DoBench<ProtoMessage2>(uint.Parse(args[0]), testData);
        }
    }
}