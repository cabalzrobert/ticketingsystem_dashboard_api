using System.Data;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

namespace Comm.Commons.Extensions
{
    public static class StringExt
    {
        public static String TDoQuotes(this String str)
        {
            return (str == null) ? "" : str.Replace("'", "''");
        }
        public static String TDoSingleQuotes(this String str)
        {
            return (str == null) ? "" : str.Replace("'", "\'");
        }
        public static String RemoveInLine(this String str)
        {
            return (str == null) ? "" : Regex.Replace(str, "[\r\n]*", "");
        }
        public static int countChar(this String str, char c)
        {
            int cnt = 0, i = 0;
            while (i < str.Length) if (str[i++] == c) cnt++;
            return cnt;
        }
        public static bool IsEmpty(this String str)
        {
            return String.IsNullOrEmpty(str);
        }

        //
        public static string getAccountingFormat(this String amount, String format = "0.00")
        {
            return String.Format("{0:#,##" + format + ";(#,##" + format + ");" + format + "}", amount.ToDecimalDouble());
        }
        public static string getNumberFormat(this String amount, String format = "0.00")
        {
            return amount.ToDecimalDouble().ToString(format);
        }
        
        public static double ToDecimalDouble(this String amount)
        {
            if (amount.IsEmpty()) return 0; //
            
            if ((amount.Contains("(") && amount.Contains(")")) || amount.Contains(","))
            {
                try { return Double.Parse(amount, NumberStyles.AllowParentheses | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint); }
                catch { return 0; }
            }

            try { return Double.Parse(amount); }
            catch { return 0; } 
        }
        public static bool IsDecimal(this String amount)
        {
            if (amount.IsEmpty()) return false; //

            double dbl = 0;
            if ((amount.Contains("(") && amount.Contains(")")) || amount.Contains(","))
            {
                try { dbl = Double.Parse(amount, NumberStyles.AllowParentheses | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);  dbl = 0; return true; }
                catch { return false; }
            }

            try { dbl = Double.Parse(amount); dbl = 0; return true; }
            catch { return false; }
        }
        
        /*public static string Formatter(this String str, object param)
        {
            if (str.isEmpty()) return str;
            string mergestr = "";
            List<string> splitParts = new List<string>();
            var regex = new System.Text.RegularExpressions.Regex(@"{((?<BR>{)|(?<-BR>})|(?(BR){?!})|[^{}]*)+}");
            var matches = regex.Matches(str);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                splitParts.Add(match.Value);
            }
            if (splitParts.Count != 0)
            {
                string[] splits = str.Split(splitParts.ToArray(), StringSplitOptions.None);
                string part = "";
                int cnt = splitParts.Count;
                var paramDic = param.ToDictionary();
                for (int i = 0; i < cnt; i++)
                {
                    part = "";
                    var key = splitParts[i].Substring(1, splitParts[i].Length - 2);
                    foreach (var kvp in paramDic)
                    {
                        if (kvp.Key == key.Trim())
                        {
                            part = (kvp.Value).ToString();
                            break;
                        }
                    }
                    mergestr += splits[i] + part;
                    if (i == cnt - 1)
                    {
                        if (!splits[i + 1].isEmpty())
                        {
                            mergestr += splits[i + 1];
                        }
                    }
                }
                splitParts = null; regex = null; matches = null;
                paramDic = null; splits = null;
            }
            return mergestr;
        }*/



        // HTML
        public static String TagScape(this String str)
        {
            return (str == null) ? "" : Regex.Replace(Regex.Replace(str, "<(.*)>", ""), "<(.*)>(.*)</(.*)>", "");
        }
        public static String URLEncode(this String str)
        {
            return (str == null) ? "" : System.Net.WebUtility.UrlEncode(str);
        }
        public static String URLDecode(this String str)
        {
            return (str == null) ? "" : System.Net.WebUtility.UrlDecode(str);
        }
        public static String HTMLEncode(this String str)
        {
            return (str == null) ? "" : System.Net.WebUtility.HtmlEncode(str);
        }
        public static String HTMLDecode(this String str)
        {
            return (str == null) ? "" : System.Net.WebUtility.HtmlDecode(str);
        }
    }
}