using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System;
using System.Reflection;

using System.Linq.Expressions;

using Comm.Commons.Extensions;
using System.Diagnostics;

namespace Comm.DTO
{
    public class lMapper
    {
        protected static readonly Dictionary<string, dynamic> configs;
        static lMapper()
        {
            configs = new Dictionary<string, dynamic>();
            
        }
        public static T Map<T>(object source)
        {
            string name = $"{ source.GetType().FullName }|{ typeof(T).FullName }";
            if(!configs.ContainsKey(name))
                Register(typeof(T), source.GetType());
            return ((T)configs[name].Action(source));
        }

        public static bool Register<TCast, TSource>()
        {
            return Register(typeof(TCast), typeof(TSource));
        }
        public static bool Register(Type TCast, Type TSource)
        {
            var name = $"{ TSource.FullName }|{ TCast.FullName }";
            if(!configs.ContainsKey(name))
            {
                var config = Dynamic.Object;
                config.Mapper = new MapperConfiguration(cfg => {
                    cfg.CreateMap(TSource, TCast);
                });
                config.Action = (Func<object, object>)((object source) => {
                    IMapper mapper = config.Mapper.CreateMapper(); 
                    try { return mapper.Map(source, TSource, TCast); }
                    finally{ mapper = null; }
                });
                configs[name] = config;
            }
            return false;
        }
        public static IMapper Create(Action<IMapperConfigurationExpression> configure)
        {
            var mapperCfg = new MapperConfiguration(configure);
            return mapperCfg.CreateMapper();
        }
    }
}