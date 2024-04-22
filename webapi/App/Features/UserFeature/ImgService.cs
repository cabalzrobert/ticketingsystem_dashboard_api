using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Configuration;
using webapi.Commons.AutoRegister;
using Comm.Commons.Extensions;

namespace webapi.App.Features.UserFeature
{
    [Service.Singleton(true)]
    public class ImgService
    {   
        public static string UrlHost = "http://localhost/upload.php";
        public ImgService(IConfiguration config){
            ImgService.UrlHost = config["ImgService:Url"].Str();
        }

        public static async Task<string> SendAsync(byte[] imageBytes, int retry = 2)
        {
            WebRequest request = null;
            string responseText = null;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request = WebRequest.Create(new Uri(ImgService.UrlHost));
                request.Method = "POST";
                request.ContentType = "application/json-patch+json; application/x-www-form-urlencoded";
                request.ContentLength = imageBytes.Length;
                using (var stream = request.GetRequestStream())
                    stream.Write(imageBytes, 0, imageBytes.Length);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var encoding = Encoding.GetEncoding(response.CharacterSet);
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseText = reader.ReadToEnd();
                    }
                    response.Close();
                }
            }catch(Exception e){
                if(retry>0)
                    return await SendAsync(imageBytes, (retry-1));
            }finally{
                request = null;
            }
            return responseText;
        }
    }
}