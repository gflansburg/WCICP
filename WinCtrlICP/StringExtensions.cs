using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WinCtrlICP
{
    internal static class StringExtensions
    {
        public static bool IsIPAddress(this string str)
        {
            IPAddress? iPAddress;
            return IPAddress.TryParse(str, out iPAddress);
        }
    }
}
