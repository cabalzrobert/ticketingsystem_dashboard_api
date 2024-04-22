using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Text.RegularExpressions;
using System.Linq;
using Comm.Commons.Advance;
using System.Text;
using System.Xml.Serialization;

namespace webapi.App.Aggregates.Common
{
    public class AggrUtils
    {
        public static string Encrypt(string text){
            String encryptV2 = null; 
            if(!text.IsEmpty()){
                do{
                    if(text.Equals(DecryptV2(encryptV2=EncryptV2(text))))
                        break;
                }while(true);
            }
            return encryptV2;
        }
        public static string Decrypt(string encryptV2){
            return DecryptV2(encryptV2);
        }
        private static string EncryptV2(string text){
            Random rand = new Random();
            int randInt = rand.Next(text.Length-3);
            var charAt = text[randInt];
            var chars = text.Split(charAt).Where(s=>!s.IsEmpty()).ToArray<string>();
            var pickChar = chars[rand.Next(chars.Length - 1)];
            var holder = text.Substring(randInt, 2);
            var sb = new StringBuilder(pickChar + holder);
            var encrypt = Cipher.Encrypt(text, sb.ToString());
            var str = Regex.Replace((sb.ToString() + encrypt), "[/+=]", "");
            var minChar = str.Substring(0, str.Length-6).GroupBy(c => c).Select(c =>new { Char = c.Key, Count = c.Count()})
                .Aggregate((a,b)=>(a.Count<b.Count?a:b)).Char;
            var charAt3 = str.IndexOf(minChar);
            sb.Append(String.Join("", str.Substring(charAt3+1, 5).Reverse())).Append(encrypt);
            return sb.ToString();
        }
        private static string DecryptV2(string encrypt){
            try{
                var str = Regex.Replace(encrypt, "[/+=]", "");
                var minChar = str.Substring(0, str.Length-6).GroupBy(c => c).Select(c =>new { Char = c.Key, Count = c.Count()})
                    .Aggregate((a,b)=>(a.Count<b.Count?a:b)).Char;
                var charAt1 = str.IndexOf(minChar);
                var active = String.Join("", str.Substring(charAt1+1, 5).Reverse());
                int charAt2 = encrypt.IndexOf(active);
                if(charAt2>-1)
                    return Cipher.Decrypt(encrypt.Substring(charAt2 + active.Length), encrypt.Substring(0, charAt2));
            }catch{}
            return null;
        }

        public class Number
        {
            public static double Floor(double d, int decimals = 2){
                var power =  Math.Pow(10, decimals);
                return ((int)Math.Floor(d * power)) / power;
            }
        }

        public class YouTube
        {
            public static string GetVideoId(string url) {
                var regex = @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?|watch)\/|.*[?&amp;]v=)|youtu\.be\/)([A-Za-z0-9\\-]{11})"; //([^""&amp;?\/ ]{11})
                var match = Regex.Match(url, regex);
                if (match.Success)
                    return match.Groups[1].Value;
                return null;
            }
        }

        public class Text
        {
            public static string Get(string[] arr, int index){
                return arr.Length>index?arr[index]:null;
            }
        }
        
        public class Xml
        {
            public static StringBuilder toXmlString<T>(List<T> list){
                StringBuilder sb = new StringBuilder();
                foreach(var xmlObj in list)
                    sb.Append(toXmlString(xmlObj));
                return sb;
            }
            public static StringBuilder toXmlString<T>(IEnumerable<T> list){
                StringBuilder sb = new StringBuilder();
                foreach(var xmlObj in list)
                    sb.Append(toXmlString(xmlObj));
                return sb;
            }
            public static StringBuilder toXmlString<T>(T xmlObj){
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces(); ns.Add("","");
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(xmlObj.GetType());
                serializer.Serialize(stringwriter, xmlObj, ns);
                return stringwriter.GetStringBuilder().Remove(0, 40);
            }
        }
    }
}   