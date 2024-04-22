using System.Data;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using System.Dynamic;
namespace Comm.Commons.Extensions
{
    public static class ObjectExt
    {
        public static T Ref<T>(this Object source, Action<T> action)
        {
            try { action((T)source); }catch{}
            action = null;
            return (T)source;
        }

        public static T To<T>(this Object source, bool isnullable = true)
        {
            return (T)(ObjTo<T>(source, isnullable));
        }

        public static T To<T>(this JObject source, bool isnullable = true)
        {
            return (T)(ObjTo<T>(source, isnullable));
        }
    
        public static T To<T>(this JArray source, bool isnullable = true)
        {
            return (T)(ObjTo<T>(source, isnullable));
        }

        public static T ObjTo<T>(object source, bool isnullable = true)
        {
            try
            {
                if(source!=null){
                    if (source is JsonElement){
                        String str = (source as JsonElement?).ToString();
                        source = JsonConvert.DeserializeObject<T>(str);
                        str = null;
                    }else if (source is JObject)
                        source = (source as JObject).ToObject<T>();
                    else if (source is JArray)
                        source = (source as JArray).ToObject<T>();
                    else if (source is T)
                        source = (T)source;
                    else if(source is IDynamicMetaObjectProvider){
                        try{
                            String str = JsonConvert.SerializeObject(source);
                            source = JsonConvert.DeserializeObject<T>(str);
                            str = null;
                        } catch { source = null; }
                    }else source = null;
                }
                if (!isnullable && source == null)
                    source = Activator.CreateInstance(typeof(T));
                return (T)(source);
            }
            catch{ source=default(T); }
            return (T)(source);
        }

        // advance
        public static string Str(this object source) { return (source ?? "").ToString(); }
        public static bool Bool(this object source) { try { return (bool)source; } catch { return false; } }
        /*
        public static bool Has(this object settings, string name) //IsPropertyExist
        {
            if (settings is System.Dynamic.ExpandoObject)
                return ((IDictionary<string, object>)settings).ContainsKey(name);
            return settings.GetType().GetProperty(name) != null;
        }
        */
        private static System.Reflection.MethodInfo mInfo;
        private static System.Reflection.FieldInfo gInfo;
        public static bool Has(this object settings, string name) //IsPropertyExist
        {
            gInfo = settings.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            mInfo = settings.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return (gInfo != null || mInfo != null);
        }

        public static void Call(this object settings, string name, params object[] args) //IsPropertyExist
        {
            mInfo = settings.GetType().GetMethod(name);
            if (mInfo != null) mInfo.Invoke(settings, args);
        }

        public static Dictionary<string, object> ToDictionary(this IDictionary<string, object> source)
        {
            return (new Dictionary<string, object>(source));
        }

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();

        }
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = Dynamic.Object;
            foreach (System.ComponentModel.PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));
            return (expando as System.Dynamic.ExpandoObject);
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
                ThrowExceptionWhenSourceArgumentIsNull();
            string typename = source.GetType().Name;

            if (typename == "Dictionary`2") return (Dictionary<string, T>)source;
            else if (typename == "ExpandoObject") return (IDictionary<string, T>)source;

            var dictionary = new Dictionary<string, T>();
            foreach (System.ComponentModel.PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(source))
                AddPropertyToDictionary<T>(property, source, dictionary);

            return dictionary;
        }


        private static void AddPropertyToDictionary<T>(System.ComponentModel.PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
        {
            object value = property.GetValue(source);
            if (IsOfType<T>(value))
                dictionary.Add(property.Name, (T)value);
        }
        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
        }

    }
}