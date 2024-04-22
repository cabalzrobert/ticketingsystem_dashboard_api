using System;
using Microsoft.AspNetCore.Mvc;
using webapi.App.Model.User;

using webapi.Commons.AutoRegister;
using Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using Comm.Commons.Extensions;
using Comm.Commons.Advance;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using Newtonsoft.Json;
//using System.Web.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using webapi.App.Aggregates.Common;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

using System.Text;
using System.Collections.Specialized;
using Microsoft.IdentityModel.Tokens; 
using System.Net.Http;
using System.Net.Http.Headers;

namespace webapi.App.Features.UserFeature
{
    [Service.Singleton]
    public class Pusher
    {   
        public static string PublicKey = "0123456789ABCDEF";
        public static string UrlHost = "localhost";
        public Pusher(IConfiguration config
            // register other singleton class
            , StompPusher stomp){
            Pusher.UrlHost = config["Pusher:Localhost"].Str();
            Pusher.PublicKey = config["Pusher:Key"].Str();
        }

        public static async Task<bool> PushAsync(string topic, object data){
            string body = "";
            if(data != null){
                if(data is String) body = (data as string);
                else body = JsonConvert.SerializeObject(data);
            }
            if(body.IsEmpty())return false;
            var json = JsonConvert.SerializeObject(new { topic = topic, body = body });
            data = Encoding.UTF8.GetBytes(json);   //Encoding.ASCII
            Timeout.Set(async()=>await SendAsync($"http://{UrlHost}/v1/{PublicKey}", data as byte[]), 75);
            json = null; body = null;
            return true;
        }
        public static async Task<bool> SendAsync(string url, byte[] data, int retry = 2){
            WebRequest request = null;
            try{
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request = WebRequest.Create(new Uri(url));
                request.Method = "POST";
                request.ContentType = "application/json-patch+json;"; // application/x-www-form-urlencoded
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);
                using (var response = (HttpWebResponse)request.GetResponse())
                    response.Close();
                data.Clear();
                return true;
            }catch(Exception e){
                if(retry>0)
                    return await SendAsync(url, data, (retry-1));
                data.Clear();
            }finally{
                request = null;
            }
            return false;
        }
    }
}

        /*public static async Task<bool> PushAsync(string topic, object data){
            string body = "";
            if(data != null){
                if(data is String) body = (data as string);
                else body = JsonConvert.SerializeObject(data);
            }
            if(body.IsEmpty())return false;
            return await SendAsync(topic, body);
        }*/
/*

        public static async Task<bool> SendAsync(string topic, string body, int retry = 2){
            try{
                using (var client = new HttpClient(new HttpClientHandler{ UseProxy=false })){
                    client.BaseAddress = new Uri("http://" + urlHost);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var httpContent = new StringContent(JsonConvert.SerializeObject(new { topic=topic, body=body }), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync($"/v1/{PublicKey}", httpContent);
                    return result.StatusCode.Equals(HttpStatusCode.OK);
                }
            }catch{
                if(retry>0)
                    return await SendAsync(topic, body, (retry-1));
            }
            return false;
        }

*/