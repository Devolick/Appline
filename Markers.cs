using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Appline
{
    internal static class Markers
    {
        public const string EMPTY = "0";
        public const string MSG = "m";
        public const string JSON = "j";
        public const string XML = "x";
        public const string SYNC = "s";

        internal static string UnMark(this string str)
        {
            Regex regex = new Regex("^[0mjxs]");
            if (regex.IsMatch(str)) { str = regex.Replace(str, string.Empty); }
            return str;
        }
    }
}
