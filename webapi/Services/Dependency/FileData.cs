using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Data;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Comm.Commons.Extensions;
using webapi.Commons.AutoRegister;

namespace webapi.Services.Dependency
{
    
    [Service.ISingleton(typeof(FileData))] 
    public interface IFileData
    {
        T Data<T>(string key, bool isnullable = true);
        string String(string key);
    }
    public class FileData : IFileData
    {   
        //private Dictionary<string, object> fileData;
        public IConfiguration Configuration { get; } 
        public FileData(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("data.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();  
        }
        public string String(string key){
            return Configuration[key].Str();
        }
        public T Data<T>(string key, bool isnullable = true){
            object source = null;
            try{
                string str = String(key);
                if(!str.IsEmpty())
                    try{ source = JsonConvert.DeserializeObject<T>(str); }catch{}
                if (!isnullable && source == null)
                    source = System.Activator.CreateInstance(typeof(T));
                return (T)source;
            }catch{ source=default(T); }
            return (T)source;
        }

    }
}