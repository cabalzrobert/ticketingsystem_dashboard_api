using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace webapi.Services
{
    public static class SwaggerServices
    {
        private static bool AllowSwagger = false;
        public static void GetSwaggerServices(this IServiceCollection services, IConfiguration config)
        {
            try{
                if(config["Swagger:Full"].Equals("*"))
                    AllowSwagger = true;
            }catch{}
            if(!AllowSwagger)return;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo  { Title = "My API", Version = "v1" }); //Info
                //c.CustomSchemaIds(i => i.FullName.Replace(i.Namespace, "")); // original
                c.CustomSchemaIds(x => x.FullName); // UseFullTypeNameInSchemaIds replacement for .NET Core
                c.AddSecurityDefinition("Bearer", //Name the security scheme
                    new OpenApiSecurityScheme{
                        Description = "JWT Authorization header using the Bearer scheme.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey, 
                        Scheme = "Bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement{ 
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Bearer", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                });
                c.OperationFilter<ApiAuthorizationHeaderParameterOperationFilter>();
            });
        }
        public static void GetSwaggerBuilders(this IApplicationBuilder app)
        {
            if(!AllowSwagger)return;
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MLM API v1");
            });
        }

        public class ApiAuthorizationHeaderParameterOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation  operation, OperationFilterContext context)
            {
                var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
                var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
                var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

                //if (isAuthorized && !allowAnonymous)
                //{}
                if (operation.Parameters == null)
                    operation.Parameters = new List<OpenApiParameter>();
                operation.Parameters.Add(new OpenApiParameter 
                {
                    Name = "Token",
                    In = ParameterLocation.Header,
                    Description = "Token",
                    //Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "String",
                        Default = new OpenApiString(""),
                    }
                });
            }
        }
    }
}