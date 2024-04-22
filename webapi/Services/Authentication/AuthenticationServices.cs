using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;
using System;
using System.Text;

using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Web;
using System.Linq;

namespace webapi.Services.Authentication
{
    public static class AuthenticationServices
    {
        public static void GetAuthenticationServices(this IServiceCollection services, IConfiguration config)
        {
            string Issuer = config["TokenSettings:Issuer"]
                ,Audience = config["TokenSettings:Audience"]
                ,Key = config["TokenSettings:Key"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opts =>
            {
                opts.RequireHttpsMetadata = false;
                opts.SaveToken = true;
                //Configure() -> app.UseAuthentication(); 
                opts.TokenValidationParameters = new TokenValidationParameters   
                {
                    ValidIssuer = Issuer,//"mysite.com",
                    ValidAudience = Audience,//"mysite.com",
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                    //
                    ValidateLifetime = false,  //true,
                    RequireExpirationTime  = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });
        }
        public static void GetAuthenticationBuilders(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            //app.UseMiddleware(typeof(AuthorizationMiddleware)); use only for global
        }
    }

    /*public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public AuthorizationMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string Issuer = _config["TokenSettings:Issuer"]
                ,Audience = _config["TokenSettings:Audience"]
                ,Key = _config["TokenSettings:Key"];
            //getting the token from the URL
            var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);

            //setting the validation parameters
            //warning : these parameters should be same with the token's issuer parameters 
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,//"mysite.com",
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidAudience = Audience,//"mysite.com",
                ValidateIssuerSigningKey = true,
                RequireExpirationTime  = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
            };

            var validator = new JwtSecurityTokenHandler();

            if (!validator.CanReadToken(queryString.Get("token")) && context.Request.Method != "GET"){
                await _next.Invoke(context);
                return; 
            }
            try
            {
                //trying to parse the token 
                if(queryString.AllKeys.Contains("token")){
                    var principal = validator.ValidateToken(queryString.Get("token"), validationParameters, out var validatedToken);
                    if(principal!= null && principal.Claims.Count()!= 0){
                        context.User.AddIdentity((ClaimsIdentity) principal.Identity); //context.User.Claims[0] = (ClaimsIdentity) principal.Identity;
                    }
                }
                await _next.Invoke(context);
                / *if (principal.HasClaim(c => c.Type == ClaimTypes.UserData))
                {
                    var userData = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;

                    //setting AuthData to be used for later usages in other middlewares
                    //as well as the controller which is in the MVC middleware 
                    //context.Items["AuthData"] = JsonConvert.DeserializeObject<LoginModel>(userData);

                    //authorizing the http context 
                    context.User.AddIdentity((ClaimsIdentity) principal.Identity);

                    //invoking the next middleware 
                    await _next.Invoke(context);
                }* /
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    public class HeaderOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        readonly string _name;
        public HeaderOAuthBearerProvider(string name)
        {
            _name = name;
        }
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var value = context.Request.Headers.Get(_name);
            if (!string.IsNullOrEmpty(value))
            {
                context.Token = value;
            }
            return Task.FromResult<object>(null);
        }
    }
    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        readonly string _name;
        public QueryStringOAuthBearerProvider(string name)
        {
            _name = name;
        }
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var value = context.Request.Query.Get(_name);
            if (!string.IsNullOrEmpty(value))
            {
                context.Token = value;
            }
            return Task.FromResult<object>(null);
        }
    }*/
}