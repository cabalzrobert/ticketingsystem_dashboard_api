using System.Data;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using System.Text;
namespace Comm.Commons.Extensions
{
    public static class ExtraExt
    {
        public static void newColumn(this DataTable dt, String col_name)
        {
            DataColumn dc = new DataColumn(col_name, typeof(String));
            dt.Columns.Add(dc);
        }
        public static void newRowValueString(this DataTable dt, String[] arrStr)
        {
            DataRow dr = dt.NewRow();
            for (int i = 0; i < arrStr.Length; i++) dr[i] = arrStr[i];
            dt.Rows.Add(dr);
        }

        public static String[] Insert(this string[] arr, String str)
        {
            String[] temp = new String[1];
            if (arr != null) temp = new String[arr.Length + 1];
            for (int i = 0; i < arr.Length; temp[i] = arr[i], i++) ;
            temp[arr.Length] = str;
            return temp;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static String DBDateToDateString(this string MMddyyyy, String format = "MM/dd/yyyy", bool allowDefault = false)
        {
            String str = ""; bool isok = false;
            tmp_dt = DateTime.Now;
            if (!MMddyyyy.IsEmpty())
            {
                try { tmp_dt = DateTime.Parse(MMddyyyy); isok = true; }
                catch { tmp_dt = DT1900; }
            }
            if (isok)
            {
                if (tmp_dt.Year < 1901) str = "";
                else str = tmp_dt.ToString(format);
            }
            else if (allowDefault) str = tmp_dt.ToString(format);

            return str;
        }

        private static string[] arr_format = new[] {
            "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy",
            "MM-dd-yyyy", "M-d-yyyy", "M-dd-yyyy", "MM-d-yyyy",
            "yyyy-MM-dd", "yyyy-MM-d", "yyyy-M-dd", "yyyy-M-d",
            "yyyy/MM/dd", "yyyy/MM/d", "yyyy/M/dd", "yyyy/M/d"
        };

        public static DateTime ToDBDateTime(this string MMddyyyy)
        {
            try { return DateTime.ParseExact(MMddyyyy, arr_format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None); }
            catch { return DT1900; }
        }
        private static readonly DateTime DT1900 = new DateTime(1900, 1, 1);
        public static String ToDBDateString(this string MMddyyyy, String format = "MM/dd/yyyy", bool allowDefault = false)
        {
            String str = ""; bool isok = false;
            if (!String.IsNullOrEmpty(MMddyyyy))
            {
                try { tmp_dt = DateTime.ParseExact(MMddyyyy, arr_format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None); isok = true; }
                catch { tmp_dt = DT1900; }
                isok = true;
            }
            if (isok)
            {
                if (tmp_dt.Year < 1901) str = "";
                else str = tmp_dt.ToString(format);
            }
            else if (allowDefault) str = DateTime.Now.ToString(format);

            return str;
        }
        public static String ToDBDateTimeString(this string MMddyyyyHHmmss, String format = "MM/dd/yyyy HH:mm:ss", bool allowDefault = false)
        {
            String str = ""; bool isok = false;
            if (!MMddyyyyHHmmss.IsEmpty())
            {
                try
                {
                    dt_arr = MMddyyyyHHmmss.Split(' ');
                    tmp_dt2 = dt_arr[0].ToDBDateTime();
                    if (dt_arr.Length != 1)
                    {
                        /*var c = MMddyyyyHHmmss.Replace(dt_arr[0], "").Trim();
                        ts_temp = TimeSpan.Parse(MMddyyyyHHmmss.Replace(dt_arr[0], ""));
                        tmp_dt2 = tmp_dt2.Add(ts_temp);*/
                        ts_temp = DateTime.Parse(MMddyyyyHHmmss.Replace(dt_arr[0], "").Trim()).TimeOfDay;
                        tmp_dt2 = tmp_dt2.Add(ts_temp);
                    }
                    isok = true;
                }
                catch { tmp_dt2 = DT1900; } //(Exception ex) 
            }
            if (isok)
            {
                if (tmp_dt2.Year < 1901) str = "";
                else str = tmp_dt2.ToString(format);
            }
            else if (allowDefault) str = DateTime.Now.ToString(format);

            return str;
        }



        private static DateTime tmp_dt, tmp_dt2;
        /*
        public static String getString(this DateTime dt, String format = "MM/dd/yyyy")
        {
            if (String.IsNullOrEmpty(format)) format = "MM/dd/yyyy";
            return dt.ToString(format);
        }
        public static String getDBDateString(this String MMddyyyy, String format = "MM/dd/yyyy", bool allowDefault = false)
        {
            String str = ""; bool isok = false;
            tmp_dt = DateTime.Now;
            if (!String.IsNullOrEmpty(MMddyyyy))
            {
                if (String.IsNullOrEmpty(format)) format = "MM/dd/yyyy";
                try { tmp_dt = DateTime.ParseExact(MMddyyyy, format, System.Globalization.CultureInfo.InvariantCulture); isok = true; }
                catch { }
                if (!isok) try { tmp_dt = DateTime.Parse(MMddyyyy); isok = true; }
                    catch { }
            }
            if (isok)
            {
                if (tmp_dt.Year < 1901) str = "";
                else str = tmp_dt.ToString(format);
            }
            else if (allowDefault) str = tmp_dt.ToString(format);

            return str;
        }
        public static String getDateString(this String MMddyyyy, String format = "MM/dd/yyyy", bool allowDefault = false)
        {
            String str = ""; bool isok = false;
            tmp_dt = DateTime.Now;
            if (!String.IsNullOrEmpty(MMddyyyy))
            {
                if (String.IsNullOrEmpty(format)) format = "MM/dd/yyyy";
                try { tmp_dt = DateTime.Parse(MMddyyyy); isok = true; }
                catch (Exception ex) { }
                if (!isok)
                    try { tmp_dt = DateTime.ParseExact(MMddyyyy, new[] { "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "yyyy-MM-dd", "yyyy-MM-d", "yyyy-M-dd", "yyyy-M-d" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None); isok = true; }
                    catch { }
            }
            if (isok)
            {
                if (tmp_dt.Year < 1901) str = "";
                else str = tmp_dt.ToString(format);
            }
            else if (allowDefault) str = tmp_dt.ToString(format);

            return str;
        }*/

        private static string[] dt_arr = new string[0];
        public static String getDateTimeSpanString(this String MMddyyyyHHmmss, bool allowDefault = false)
        {
            String str = ""; bool isok = false;
            tmp_dt = DateTime.Now;
            try
            {
                dt_arr = MMddyyyyHHmmss.Split(' ');
                tmp_dt = dt_arr[0].getDateTime();
                if (dt_arr.Length != 1)
                {
                    ts_temp = TimeSpan.Parse(dt_arr.Last());
                    tmp_dt = tmp_dt.Add(ts_temp);
                }
                isok = true;
            }
            catch { }

            if (isok)
            {
                if (tmp_dt.Year < 1901) str = "";
                else str = tmp_dt.ToString("yyyy/MM/dd HH:mm:ss");
            }
            else if (allowDefault) str = tmp_dt.ToString("yyyy/MM/dd HH:mm:ss");

            return str;
        }

        public static String InString(this DateTime dt, String format = "MM/dd/yyyy", bool allowDefault = false)
        {
            String str = "";

            if (dt.Year < 1901) str = "";
            else str = dt.ToString(format);

            if (allowDefault) if (str.IsEmpty()) str = dt.ToString(format);

            return str;
        }


        public static DateTime getDateTime(this string MMddyyyy, String hhmmtt = "")
        {
            tmp_dt = DateTime.Now; bool isok = false;
            if (!String.IsNullOrEmpty(MMddyyyy))
            {
                try { tmp_dt = DateTime.ParseExact(MMddyyyy, new[] { "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "yyyy-MM-dd", "yyyy-MM-d", "yyyy-M-dd", "yyyy-M-d" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None); isok = true; }
                catch { }
                if (!isok)
                    try { tmp_dt = DateTime.Parse(MMddyyyy); isok = true; }
                    catch { }
                //; if (DateTime.TryParse(MMddyyyy, out dt))
                if (!String.IsNullOrEmpty(hhmmtt))
                {
                    tmp_dt = DateTime.Parse(tmp_dt.ToString("yyyy-MM-dd") + " " + hhmmtt);
                }
            }
            return tmp_dt;
        }
    
        
        public static DateTime ToDateTime(this string MMddyyyy, String hhmmtt = "")
        {
            tmp_dt = DateTime.Now; bool isok = false;
            if (!String.IsNullOrEmpty(MMddyyyy))
            {
                try { tmp_dt = DateTime.ParseExact(MMddyyyy, new[] { "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "yyyy-MM-dd", "yyyy-MM-d", "yyyy-M-dd", "yyyy-M-d" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None); isok = true; }
                catch { }
                if (!isok)
                    try { tmp_dt = DateTime.Parse(MMddyyyy); isok = true; }
                    catch { }
                //; if (DateTime.TryParse(MMddyyyy, out dt))
                if (!String.IsNullOrEmpty(hhmmtt))
                {
                    tmp_dt = DateTime.Parse(tmp_dt.ToString("yyyy-MM-dd") + " " + hhmmtt);
                }
            }
            return tmp_dt;
        }




        //
        private static TimeSpan ts_temp;
        public static string Duration(this DateTime dt, DateTime dt2)
        {
            //DateTime.Now
            String duration = "";
            ts_temp = (dt - dt2);
            {
                int days = ts_temp.Days;
                if (days != 0)
                {
                    if (days > 7)
                    {
                        int wk = days / 7;
                        days = days % 7;
                        duration = wk + " Week" + (wk < 2 ? "" : "s");
                    }
                    else
                    {
                        duration = days + " Day" + (days < 2 ? "" : "s");
                    }
                }
            }
            return duration;
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static double ToTimestamp(this DateTime value)
        {
            ts_temp = value - Epoch;
            return (ts_temp.TotalMilliseconds / 1000.0);
        }
        public static long ToTimeMillisecond(this DateTime value)
        {
            ts_temp = value - Epoch;
            return (long)ts_temp.TotalMilliseconds;
        }
        public static DateTime ToDateTime(this double timestamp)
        {
            return Epoch.AddMilliseconds(timestamp * 1000);
        }
        public static DateTime ToDateTime(this long timestamp)
        {
            return Epoch.AddSeconds(timestamp);
        }
        public static DateTime MilliSecondToDateTime(this double timestamp)
        {
            ts_temp = TimeSpan.FromMilliseconds(timestamp);
            return Epoch + ts_temp;
        }


        private static readonly TimeSpan utc_diff = DateTime.UtcNow - DateTime.Now;
        public static DateTime ToUtcTime(this DateTime value)
        {
            return value.Add(utc_diff);
        }
        public static DateTime UtcToTime(this DateTime value)
        {
            return value.Subtract(utc_diff);
        }

        
        public static string DateToHex(this DateTime value)
        {
            string isoDate = value.ToString("yyyyMMddHHmmssfff");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < isoDate.Length ; i++){
                int n = char.ConvertToUtf32(isoDate, i);
                sb.Append(n.ToString("x"));
            }
            return sb.ToString();  //"yyyy-MM-dd HH:mm:ss.fff
        }
    }
}