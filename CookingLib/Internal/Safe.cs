using System;
using System.Collections.Generic;

namespace CookingLib
{
    internal static class Safe
    {
        static readonly HashSet<string> logged = new HashSet<string>();

        internal static int Failures { get; private set; }

        internal static void Fail(string patch, Exception e)
        {
            Failures++;
            if (logged.Add(patch))
                Log.Error(patch + " failed, the game carries on without it: " + e);
        }
    }
}
