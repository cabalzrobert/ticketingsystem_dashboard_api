using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
namespace webapi.Commons.AutoRegister
{
    public class Service
    {
        private static readonly IEnumerable<Type> _types;
        static Service()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _types = (from type in assembly.GetTypes()
                        select type).ToList();
        }
        //--
        private static readonly Stack<Type> AutoServices = new Stack<Type>();
        public static void Init(IServiceCollection services)
        {
            var types = (from type in _types
                            where (
                                   Attribute.IsDefined(type, typeof(ITransient)) 
                                || Attribute.IsDefined(type, typeof(IScope))
                                || Attribute.IsDefined(type, typeof(ISingleton))
                                //
                                || Attribute.IsDefined(type, typeof(Transient)) 
                                || Attribute.IsDefined(type, typeof(Scope))
                                || Attribute.IsDefined(type, typeof(Singleton))
                            ) select type).ToList();
            types.ForEach(t=>
            {
                t.GetCustomAttributes(true).ToList().ForEach(a=>
                {
                    IAttribute attr = (a as IAttribute);
                    if(attr == null) return;
                    if(a is ITransient)
                        services.AddTransient(t, attr.GetInfo());
                    else if(a is IScope)
                        services.AddScoped(t, attr.GetInfo());
                    else if(a is ISingleton)
                        services.AddSingleton(t, attr.GetInfo());
                    else if(a is Transient)
                        services.AddTransient(t);
                    else if(a is Scope)
                        services.AddScoped(t);
                    else if(a is Singleton)
                        services.AddSingleton(t);
                    if(attr.IsAuto())
                        AutoServices.Push(t);
                });
            });
        }
        public static void Init(IServiceScopeFactory serviceScopeFactory)
        {
            if(AutoServices.Count < 1)return;
            using(var scope = serviceScopeFactory.CreateScope()){
                Type type = null;
                do{
                    if(AutoServices.Count<1)break;
                    type = AutoServices.Pop();
                    scope.ServiceProvider.GetService(type);
                }while(true);
            }
        }

        private interface IAttribute
        {
            bool IsAuto();
            Type GetInfo();
        }
        [AttributeUsage(AttributeTargets.Interface, AllowMultiple=true)]
        public class ITransient: Attribute, IAttribute
        {
            private bool isAuto;
            private Type info;
            public ITransient(): this(null){}
            public ITransient(Type info): this(info, false){}
            public ITransient(Type info, bool isAuto){
                this.info = info;
                this.isAuto = isAuto;
            }
            public bool IsAuto() => this.isAuto;
            public Type GetInfo() => this.info;
        }

        [AttributeUsage(AttributeTargets.Interface, AllowMultiple=true)]
        public class IScope: Attribute, IAttribute
        {
            private bool isAuto;
            private Type info;
            public IScope(): this(null){}
            public IScope(Type info): this(info, false){}
            public IScope(Type info, bool isAuto){
                this.info = info;
                this.isAuto = isAuto;
            }
            public bool IsAuto() => this.isAuto;
            public Type GetInfo() => this.info;
        }

        [AttributeUsage(AttributeTargets.Interface, AllowMultiple=true)]
        public class ISingleton: Attribute, IAttribute
        {
            private bool isAuto;
            private Type info;
            public ISingleton(): this(null){}
            public ISingleton(Type info): this(info, false){}
            public ISingleton(Type info, bool isAuto){
                this.info = info;
                this.isAuto = isAuto;
            }
            public bool IsAuto() => this.isAuto;
            public Type GetInfo() => this.info;
        }


        [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
        public class Transient: Attribute, IAttribute
        {
            private bool isAuto;
            public Transient(): this(false){}
            public Transient(bool isAuto){
                this.isAuto = isAuto;
            }
            public bool IsAuto() => this.isAuto;
            public Type GetInfo() => null;
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
        public class Scope: Attribute, IAttribute
        {
            private bool isAuto;
            public Scope(): this(false){}
            public Scope(bool isAuto){
                this.isAuto = isAuto;
            }
            public bool IsAuto() => this.isAuto;
            public Type GetInfo() => null;
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
        public class Singleton: Attribute, IAttribute
        {
            private bool isAuto;
            public Singleton(): this(false){}
            public Singleton(bool isAuto){
                this.isAuto = isAuto;
            }
            public bool IsAuto() => this.isAuto;
            public Type GetInfo() => null;
        }
    }

}