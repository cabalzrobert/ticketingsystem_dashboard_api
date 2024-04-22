using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Configuration;
using webapi.Commons.AutoRegister;
using Comm.Commons.Extensions;

namespace webapi.App.Features.UserFeature
{
    [Service.Singleton(true)]
    public class ApkUploader
    {
        public static string UrlHost = "http://localhost/upload.php";
        public ApkUploader(IConfiguration config)
        {
            ApkUploader.UrlHost = config["ApkUploader:Url"].Str();
        }
        public static async Task<string> SendAsync(string type, string version, byte[] fileBytes) //
        {
            WebRequest request = null;
            string responseText = null;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request = WebRequest.Create(new Uri(ApkUploader.UrlHost + "=" + type + "&version=" + version));
                request.Method = "POST";
                request.ContentType = "application/json-patch+json; application/x-www-form-urlencoded";
                request.ContentLength = fileBytes.Length;
                using (var stream = request.GetRequestStream())
                    stream.Write(fileBytes, 0, fileBytes.Length);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var encoding = Encoding.GetEncoding(response.CharacterSet);
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseText = reader.ReadToEnd();
                    }
                    response.Close();
                }
            }
            catch { }
            return responseText;
        }
    }
}
