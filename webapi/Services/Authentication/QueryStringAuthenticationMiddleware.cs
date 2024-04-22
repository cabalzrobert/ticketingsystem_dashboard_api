using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using webapi.Commons.AutoRegister;
using System.Collections.Specialized;

namespace webapi.Services.Authentication
{
    [Service.IScope(typeof(QueryStringAuthenticationMiddleware))] 
    public interface IQueryStringAuthenticationMiddleware
    {
        bool Validate(ClaimsPrincipal claims, NameValueCollection queryString);
    }
    public class QueryStringAuthenticationMiddleware : IQueryStringAuthenticationMiddleware
    {   
        private readonly IConfiguration _config;
        public QueryStringAuthenticationMiddleware(IConfiguration config)
        {
            _config = config;
        }
        
        public bool Validate(ClaimsPrincipal claims, NameValueCollection queryString){
            string Issuer = _config["TokenSettings:Issuer"]
                ,Audience = _config["TokenSettings:Audience"]
                ,Key = _config["TokenSettings:Key"];

            var validationParameters = new TokenValidationParameters{
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

            var token = queryString.Get("token");
            var validator = new JwtSecurityTokenHandler();
            if(!validator.CanReadToken(token))
                return false;

            var principal = validator.ValidateToken(token, validationParameters, out var validatedToken);
            if(principal != null && principal.Claims.Count() != 0){
                claims.AddIdentity((ClaimsIdentity) principal.Identity);
                return true;
            }
            return false;
        }
    }
}