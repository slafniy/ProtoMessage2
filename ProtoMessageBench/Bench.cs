using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoMessageOriginal;

namespace ProtoMessageBench
{
    internal static class Bench
    {
        public static string ProtoAsText =
            "root_message {\n"
            + "  single_int: 123\n"
            + "  single_double: 123.2342425\n"
            + "  single_submessage {\n"
            + "    single_string: \"some string value\"\n"
            + "    repeated_int: 638427648\n"
            + "    repeated_int: 63842723138\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 21231231\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 2321543\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 4324\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 243553321543\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 6544\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 456\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 45644\n"
            + "    submessage_single_int2: 45644\n"
            + "    submessage_single_int3: 45644\n"
            + "    submessage_single_int4: 45644\n"
            + "    submessage_single_int5: 45644\n"
            + "    submessage_single_int6: 45644\n"
            + "    submessage_single_int7: 45644\n"
            + "    submessage_single_int8: 45644\n"
            + "    submessage_single_int9: 45644\n"
            + "    submessage_single_int10: 45644\n"
            + "    submessage_single_int11: 45644\n"
            + "    submessage_single_int12: 45644\n"
            + "    submessage_single_int13: 45644\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 45644\n"
            + "    submessage_single_int2: 45644\n"
            + "    submessage_single_int3: 45644\n"
            + "    submessage_single_int4: 45644\n"
            + "    submessage_single_int5: 45644\n"
            + "    submessage_single_int6: 45644\n"
            + "    submessage_single_int7: 45644\n"
            + "    submessage_single_int8: 45644\n"
            + "    submessage_single_int9: 45644\n"
            + "    submessage_single_int10: 45644\n"
            + "    submessage_single_int11: 45644\n"
            + "    submessage_single_int12: 45644\n"
            + "    submessage_single_int13: 45644\n"
            + "  }\n"
            + "  repeated_submessage {\n"
            + "    submessage_single_int: 45644\n"
            + "    submessage_single_int2: 45644\n"
            + "    submessage_single_int3: 45644\n"
            + "    submessage_single_int4: 45644\n"
            + "    submessage_single_int5: 45644\n"
            + "    submessage_single_int6: 45644\n"
            + "    submessage_single_int7: 45644\n"
            + "    submessage_single_int8: 45644\n"
            + "    submessage_single_int9: 45644\n"
            + "    submessage_single_int10: 45644\n"
            + "    submessage_single_int11: 45644\n"
            + "    submessage_single_int12: 45644\n"
            + "    submessage_single_int13: 45644\n"
            + "  }\n"
            + "}";

        public static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (uint i = 0; i < uint.Parse(args[0]); i++)
            {
                var pm = new ProtoMessage();
                pm.Parse(ProtoAsText);

                pm.GetKeys();
                ProtoMessage root = pm.GetElement("root_message");
                var intAttr = root.GetAttribute<int>("single_int");
                double? doubleAttr = root.GetAttributeOrNull<double>("single_double");
                ProtoMessage singleMsg = root.GetElement("single_submessage");
                List<string> repeatedInt = singleMsg.GetAttributeList("repeated_int");
                List<ProtoMessage> repeatedSubMsg = root.GetElementList("repeated_submessage");
                var subMsgInt = repeatedSubMsg[1].GetAttribute<ulong>("submessage_single_int");
            }
            stopwatch.Stop();
            Console.WriteLine($"{args[0]} iterations finished in {stopwatch.Elapsed.TotalSeconds} sec");
        }
    }
}