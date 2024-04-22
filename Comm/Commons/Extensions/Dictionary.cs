using System.Data;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text.Json;
namespace Comm.Commons.Extensions
{
    public static class DictionaryExt
    {   

        public static string GetValue<T>(this Dictionary<T, string> dic, T key, bool allownull = false)
        {
            if (dic.ContainsKey(key)) return dic[key];
            return allownull ? null : "";
        }
        
        public static string GetKey(this Dictionary<string, string> dic, String value, bool allownull = false)
        {
            foreach (var item in dic) if (item.Value == value) return item.Key;
            return allownull ? null : "";
        }

        public static string GetKey(this Dictionary<string, object> dic, object value, bool allownull = false)
        {
            foreach (var item in dic) if (item.Value == value) return item.Key;
            return allownull ? null : "";
        }

        public static object GetValue(this Dictionary<string, object> dic, String key)
        {
            if (dic.ContainsKey(key)) return dic[key];
            return null;
        }
        public static T GetValue<T>(this Dictionary<string, object> dic, String key, bool isnullable = true)
        {
            object obj = null;
            bool haskey = dic.ContainsKey(key);
            try {
                if (haskey){
                    obj = dic[key];
                    if(obj is JsonElement || obj is JObject || obj is JArray)
                        obj = obj.To<T>();
                }
                else obj = (dic[key] = default(T));
                if (!isnullable && obj == null)
                    obj = Activator.CreateInstance(typeof(T));
                return (T)(obj);
            }
            catch{ obj=(dic[key] = default(T)); }
            return (T)(obj);
        }
        public static T Get<T>(this Dictionary<string, object> dic, String key, bool isnullable = true, object defaultValue = null)
        {
            object obj = null;
            bool haskey = dic.ContainsKey(key);
            if(defaultValue != null) 
                isnullable = false;
            try {
                if (haskey){
                    obj = dic[key];
                    if(obj is JsonElement || obj is JObject || obj is JArray)
                        obj = obj.To<T>();
                }
                else obj = (dic[key] = (defaultValue!=null? defaultValue: default(T)));
                if (!isnullable && obj == null)
                    obj = Activator.CreateInstance(typeof(T));
                return (T)(obj);
            }
            catch{ obj=(dic[key] = default(T)); }
            return (T)(obj);
        }

        public static void RemoveAllUsingValue(this Dictionary<string, object> dic, object value)
        {
            var keys = new List<string>();
            foreach (var keypair in dic) if (keypair.Value == value) keys.Add(keypair.Key);
            foreach (var key in keys) dic.Remove(key);
            keys = null;
        }

        public static void RemoveAllUsingValue(this Dictionary<string, string> dic, string value)
        {
            var keys = new List<string>();
            foreach (var keypair in dic) if (keypair.Value == value) keys.Add(keypair.Key);
            foreach (var key in keys) dic.Remove(key);
            keys = null;
        }




        public static Dictionary<string, object> Clone(this Dictionary<string, object> dic)
        {
            var clone = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> kvp in dic) clone.Add(kvp.Key, kvp.Value);
            return clone;
        }

        /////////////////////////////////////////////////////////////
        public static Dictionary<string, object> DeserializeObject(this Dictionary<string, object> dictionary)
        {
            String[] keys = dictionary.Keys.ToArray();
            foreach (String key in keys)
            {
                if (dictionary[key] == null) continue;
                if (dictionary[key] is JObject) dictionary[key] = ((dictionary[key] as JObject).ToObject<Dictionary<string, object>>()).DeserializeObject();
                if (dictionary[key] is JArray) dictionary[key] = ((dictionary[key] as JArray).ToObject<List<object>>()).DeserializeObject();
            }
            return dictionary;
        }

        public static List<object> DeserializeObject(this List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null) continue;
                if (list[i] is JObject) list[i] = ((list[i] as JObject).ToObject<Dictionary<string, object>>()).DeserializeObject();
                if (list[i] is JArray) list[i] = ((list[i] as JArray).ToObject<List<object>>()).DeserializeObject();
            }
            return list;
        }
    }
}