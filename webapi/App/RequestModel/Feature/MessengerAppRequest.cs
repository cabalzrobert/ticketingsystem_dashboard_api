using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Comm.Commons.Advance;
using Comm.Commons.Extensions;
using webapi.App.Model.User;
using webapi.App.Features.UserFeature;
using webapi.App.Aggregates.Common;
using Newtonsoft.Json;

namespace webapi.App.RequestModel.Feature
{
    public class MessengerAppRequest
    {
        public String Type;
        public String Message;
        public String MediaUrl;
        public bool IsImage;
        public bool IsFile;
        public bool IsMessage;
        public string MemberID;
        public string ChatID;

        /*public static bool validity(Account account, String ChatKey, MessengerAppRequest request){
            if(request == null || ChatKey.IsEmpty()) return false;
            String type = request.Type.Str();
            request.IsImage = (type == "IMG"); 
            request.IsFile = (type == "FILE");
            request.IsMessage = (type.IsEmpty() || !(request.IsImage||request.IsFile));
            
            String Message = "";
            if(request.IsMessage) Message = request.Message;
            else if(request.IsImage) Message = "{0} sent a photo.";
            else if(request.IsFile) Message = "{0} sent a file.";
            //
            request.MediaUrl = "";
            if(request.IsImage){
                if(!validity(request).Result){
                    var filename = $"{ Cipher.MD5Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")) }.png";
                    var fullpath = $"./wwwroot/images/{ChatKey}/{ filename }";
                    byte[] bytes = Convert.FromBase64String(request.Message);
                    (new System.IO.FileInfo(fullpath).Directory).Create();
                    using (BinaryWriter binWriter = new BinaryWriter(System.IO.File.Open(fullpath, FileMode.Create)))  
                        binWriter.Write(bytes, 0, bytes.Length);
                    bytes.Clear();
                    request.MediaUrl = ($"/images/{ChatKey}/{filename}");
                }
            }
            request.Message = Message;
            return true;
        }

        private static async Task<bool> validity(MessengerAppRequest request){
            if(request==null)
                return false;
            //if(!request.ImageUrl.IsEmpty())
            //    return (Results.Success, null);

            if(request.Message.IsEmpty())
                return false;

            byte[] bytes = Convert.FromBase64String(request.Message.Str());
            if(bytes.Length == 0)
                return false;
            
            var res = await ImgService.SendAsync(bytes);
            bytes.Clear();
            if(res==null)
                return false;

            var json = JsonConvert.DeserializeObject<Dictionary<string,object>>(res);
            if(json["status"].Str()!="error"){
                request.MediaUrl = json["url"].Str();
                return true;
            }
            return false;
        }*/
    }
}