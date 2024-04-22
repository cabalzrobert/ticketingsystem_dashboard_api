using System.Data;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using System.Text;
namespace Comm.Commons.Extensions
{
    public static class NumberExt
    {
        /*
            short 2 bytes -32,768 to 32,767.
            int 4 bytes -2,147,483,648 to 2,147,483,647.
            long 8 bytes -9,223,372,036,854,775,808 to 9,223,372,036,854,775,80.
            float 4 bytes 7 decimal digits.
            double 8 bytes 16 decimal digits.
        */
        public static string ToAccountingFormat(this short n, String format = "0.00") 
        {
            return String.Format("{0:#,##" + format + ";(#,##" + format + ");" + format + "}", n);
        }
        public static string ToAccountingFormat(this int n, String format = "0.00") 
        {
            return String.Format("{0:#,##" + format + ";(#,##" + format + ");" + format + "}", n);
        }
        public static string ToAccountingFormat(this long n, String format = "0.00") 
        {
            return String.Format("{0:#,##" + format + ";(#,##" + format + ");" + format + "}", n);
        }
        public static string ToAccountingFormat(this float n, String format = "0.00") 
        {
            return String.Format("{0:#,##" + format + ";(#,##" + format + ");" + format + "}", n);
        }
        public static string ToAccountingFormat(this double n, String format = "0.00") 
        {
            return String.Format("{0:#,##" + format + ";(#,##" + format + ");" + format + "}", n);
        }

        public static string ToFixed(this int n, int fixn)
        {
            return ((double)n).ToFixed(fixn);
        }
        public static string ToFixed(this long n, int fixn)
        {
            return ((double)n).ToFixed(fixn);
        }
        public static string ToFixed(this float n, int fixn)
        {
            return ((double)n).ToFixed(fixn);
        }
        public static string ToFixed(this double n, int fixn)
        {
            if (fixn == 0) return ((int)Math.Round(n, 0)).ToString();
            else if (fixn > 0) return Math.Round(n, fixn).ToString("N" + fixn);
            else
            {
                var pow10 = Math.Pow(10, fixn * -1);
                return ((int)Math.Round(n / pow10, 0) * pow10).ToString();
            }
        }

        private static SortedDictionary<int, string> valueMapRoman = new SortedDictionary<int, string>{
            { 1, "I" },
            { 4, "IV" },
            { 5, "V" },
            { 9, "IX" },
            { 10, "X" },
            { 40, "XL" },
            { 50, "L" },
            { 90, "XC" },
            { 100, "C" },
            { 400, "CD" },
            { 500, "D" },
            { 900, "CM" },
            { 1000, "M" },
        };
        public static string ToRomanNumeral(this int number)
        {
            var retVal = new StringBuilder(5);
            foreach (var kvp in valueMapRoman.Reverse())
            {
                while (number >= kvp.Key)
                {
                    number -= kvp.Key;
                    retVal.Append(kvp.Value);
                }
            }
            return retVal.ToString();
        }
    }
}