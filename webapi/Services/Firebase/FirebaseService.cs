using System;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Comm.Commons.Extensions;
using System.Collections.Generic;

namespace webapi.Services.Firebase
{
    public static class FirebaseService
    {
        public static async Task<bool> SendAsync(string serverToken, object token = null, string topic = null,  string condition = null, object notification = null, object data = null, int retry = 1){
            if(true)return true;
            try{
                using (var client = new HttpClient()){
                    client.BaseAddress = new Uri("https://fcm.googleapis.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",$"key={serverToken}");
                    dynamic json = Dynamic.Object;
                    if(!condition.IsEmpty()) json.condition = condition.URLDecode();
                    else if(!topic.IsEmpty()) json.to = topic.URLDecode();
                    else if(token!=null) {
                        if(token is String) json.registration_ids = new String[]{ token.Str().URLDecode() };
                        else json.registration_ids = token;
                    }
                    if(notification !=null) json.notification = notification;
                    if(data == null) data = new{};
                    json.data = data;
                    var httpContent = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync("/fcm/send", httpContent);
                    return result.StatusCode.Equals(HttpStatusCode.OK);
                }
            }catch{
                if(retry>0)
                    return await SendAsync(serverToken, token:token, topic:topic, condition:condition, notification:notification, data:data, retry:(retry-1));
            }
            return false;
        }
        public static async Task<bool> SendMultipleAsync(string serverToken, List<string> tokens, object notification = null, object data = null){
            if(true)return true;
            try {
                List<string> temp = new List<string>();
                do{
                    for(int i=0;i<tokens.Count;i++){
                        temp.Add(tokens[i]);
                        if(temp.Count==1750)
                            break;
                    }
                    tokens.RemoveRange(0, temp.Count);
                    var response = await SendAsync(serverToken, token:tokens, notification:notification, data:data);
                    if(!response) return false;
                    temp.Clear();
                    if(tokens.Count<1)
                        break;
                }while(true);
                return true;
            }catch{}
            return false;
        }
    }
}