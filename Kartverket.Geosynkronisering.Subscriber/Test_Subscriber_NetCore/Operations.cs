﻿using System.Collections.Generic;

namespace Test_Subscriber_NetCore
{
    internal class Operations
    {
        internal const string help = "help";
        internal const string add = "add";
        //internal static string sync = "sync";
        internal const string auto = "auto";
        internal const string reset = "reset";
        internal const string remove = "remove";
        internal const string list = "list";
        internal const string sync = "sync";
        
        internal static List<string> all = new List<string> { help, add, list, auto, sync, reset, remove };
    }
}