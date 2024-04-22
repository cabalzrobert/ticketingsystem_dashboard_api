using System;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
///using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;

using Comm.Commons.Extensions;
namespace Comm.Commons.RegularExpressions
{
    public class RegEx
    {
        public static string TryGetDate(String strline, ref Regex RegEx, bool withtime = false)
        {
            String strdate_output = "";
            if (!String.IsNullOrEmpty(strline))
            {
                if (RegEx == null) RegEx = new Regex("");
                var option = RegexOptions.IgnoreCase;

                if (!RegEx.ToString().IsEmpty())
                {
                    strdate_output = RegEx.Match(strline).ToString();
                }
                else
                {
                    string[] sp_char = new string[] { "/", "-", "\\." };
                    String[] date_format = new string[] { @"(\d{1,2})_(\d{1,2})_(\d{4})", @"(\d{4})_(\d{1,2})_(\d{1,2})" };

                    String reg_time_24hr_format = @"(\d{1,2}):(\d{2}):(\d{2})";
                    String reg_time_12hr_ampm_format = @"(\d{1,2}):(\d{2})[ ]{0,1}(AM|PM)";
                    String reg_time_24hr_ampm_format = (reg_time_24hr_format + "[ ]{0,1}(AM|PM)");


                    for (int i = 0; i < date_format.Length; i++)
                    {
                        if (!String.IsNullOrEmpty(strdate_output)) break;

                        for (int j = 0; j < sp_char.Length; j++)
                        {
                            string reg_date_format = date_format[i].Replace("_", sp_char[j]);

                            if (!withtime)
                            {
                                RegEx = new Regex(reg_date_format, option); //  date + 24hr am pm recognize date and time
                                strdate_output = RegEx.Match(strline).ToString();
                                if (!String.IsNullOrEmpty(strdate_output)) break;
                            }
                            else
                            {
                                RegEx = new Regex(reg_date_format + @"[ \t\r]" + reg_time_24hr_format, option); //  date + 24hr am pm recognize date and time
                                strdate_output = RegEx.Match(strline).ToString();
                                if (!String.IsNullOrEmpty(strdate_output)) break;

                                RegEx = new Regex(reg_date_format + @"[ \t\r]" + reg_time_12hr_ampm_format, option); // date + 12hr ampm recognize date and time
                                strdate_output = RegEx.Match(strline).ToString();
                                if (!String.IsNullOrEmpty(strdate_output)) break;

                                RegEx = new Regex(reg_date_format + @"[ \t\r]" + reg_time_24hr_ampm_format, option); // date + 12hr ampm recognize date and time      |[ \t\r]0:00[ \t\r]
                                strdate_output = RegEx.Match(strline).ToString();
                                if (!String.IsNullOrEmpty(strdate_output)) break;
                                
                            }
                        }
                    }
                }
            }
            return strdate_output;
        }
        private static Regex _regex;
        public static string TryGetDate(String strline, bool withtime = false)
        {
            _regex = null;
            return TryGetDate(strline, ref _regex, withtime);
        }

    }
}


