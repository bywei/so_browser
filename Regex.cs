using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace 浏览器
{
    class MyRegex
    {
        public static string GetRegValue(string math, string text)
        {
            Regex regex = new Regex(math);
            return regex.Match(text).Value;
        }
    }
}
