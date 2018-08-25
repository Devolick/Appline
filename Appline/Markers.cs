using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Appline
{
    internal static class Markers
    {
        internal const string EMPTY = "0";
        internal const string MSG = "m";
        internal const string JSON = "j";
        internal const string XML = "x";
        internal const string SYNC = "s";

        internal static string UnMark(this string str)
        {
            Regex regex = new Regex("^[0mjxs]");
            if (regex.IsMatch(str)) { str = regex.Replace(str, string.Empty); }
            return str;
        }
    }
}
