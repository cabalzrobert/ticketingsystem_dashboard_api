using System;
using System.Collections.Generic;
using webapi.App.Model.User;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class Dtos
    {
        public static IEnumerable<dynamic> items(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>item(e));
        } 
        public static IDictionary<string, object> item(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            return o;
        }
        
    }
    
}   