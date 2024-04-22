using System.Data;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using System.Linq;
using Newtonsoft.Json.Linq;
namespace Comm.Commons.Extensions
{
    public static class Dynamic
    {
        public static dynamic Object { get { return new System.Dynamic.ExpandoObject(); } }

        public static bool PropertyExists(dynamic obj, string name)
        {
            if (obj == null) return false;
            if (obj is IDictionary<string, object>) 
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            return obj.GetType().GetProperty(name) != null;
        }
        
    }
}