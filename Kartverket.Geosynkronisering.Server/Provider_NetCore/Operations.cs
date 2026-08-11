using System.Collections.Generic;

namespace Test_Subscriber_NetCore
{
    internal class Operations
    {
        internal const string help = "help";
        internal const string initial = "initial";
        internal const string list = "list";
        internal const string push = "push";
        internal const string pushAll = "pushAll";

        internal static List<string> all = new List<string> { help, list, initial, push, pushAll };
    }
}