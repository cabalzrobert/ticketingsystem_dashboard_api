using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
namespace webapi.Services
{
    public static class CoreServices
    {
        public static void GetCoreServices(this IServiceCollection services)
        {
            webapi.Commons.AutoRegister.Service.Init(services);
            Comm.Commons.AutoRegister.Service.Init(services);
            Infrastructure.Commons.AutoRegister.Service.Init(services);
        }
        public static void GetCoreServices(this IApplicationBuilder app, IServiceScopeFactory serviceScopeFactory)
        {
            webapi.Commons.AutoRegister.Service.Init(serviceScopeFactory);
            Comm.Commons.AutoRegister.Service.Init(serviceScopeFactory);
            Infrastructure.Commons.AutoRegister.Service.Init(serviceScopeFactory);
        }
    }
}