using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using AutoMapper;
using System.Reflection;
using webapi.Services;
using webapi.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;
using webapi.App.Features.UserFeature;

namespace webapi
{
    public class Startup
    {        
        public IConfiguration Configuration { get; } 
        private IHostingEnvironment Environment { get; }
        //
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()  
              .SetBasePath(environment.ContentRootPath)  
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)  
              //.AddJsonFile($"Config/{env.EnvironmentName}.json", optional: false, reloadOnChange: true)  
              .AddEnvironmentVariables();  
            configuration = builder.Build();  
            //
            Configuration = configuration;  
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        //private readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DBContext>(c =>{
                c.UseSqlServer(Configuration["ConnectionStrings:MLM01"], options => { 
                    options.EnableRetryOnFailure();
                });  
            });

            services.GetCoreServices();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IFileProvider>(Environment.ContentRootFileProvider);
            
            //
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //
            //services.AddScoped<IResponseCachingPolicyProvider, ResponseCachingPolicyProvider>();
            services.AddResponseCaching();

            services.GetAuthenticationServices(Configuration);
            services.GetSwaggerServices(Configuration);
            services.AddAutoMapper(Assembly.Load("Infrastructure")); // 
            // default code
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddNewtonsoftJson(options=>options.SerializerSettings.ContractResolver = new DefaultContractResolver()); // remove options parameter for non validate
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceScopeFactory ssf, IHostingEnvironment env, PDFService pdfservice, ReportService reportservice, ImgService imgService, Pusher pusher)
        {
            // default code
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            else app.UseHsts(); // end
            //builders
            app.GetCoreServices(ssf);
            app.GetAuthenticationBuilders();
            app.GetSwaggerBuilders();
            //
            app.UseCors(builder => 
                builder.AllowAnyOrigin()
                    .AllowAnyHeader().AllowAnyMethod()); //.AllowCredentials()
            //app.UseCors(MyAllowSpecificOrigins);
            //
            app.GetFileExplorerServices();
            //
            app.UseWebSockets();
            //
            app.UseResponseCaching();
            // default code
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>{
                endpoints.MapControllers();
            });
        }
    }
}