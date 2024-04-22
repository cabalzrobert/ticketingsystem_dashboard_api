using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace webapi.Services
{
    public static class FileExplorerServices
    {
        public static void GetFileExplorerServices(this IApplicationBuilder app)
        {
            var fullpath = $"./wwwroot/d/apk/empty.txt";
            (new System.IO.FileInfo(fullpath).Directory).Create();
            app.UseStaticFiles(); 
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".apk"] = "application/x-msdownload";
            app.UseStaticFiles(new StaticFileOptions(){
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/d")),
                RequestPath = new PathString("/d"),
                ContentTypeProvider = provider,
            });
        }
    }
}