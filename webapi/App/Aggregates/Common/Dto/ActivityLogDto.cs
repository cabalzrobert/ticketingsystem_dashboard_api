using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class ActivityLogDto
    {
        public static IEnumerable<dynamic> FilterLoginLogs(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = LoginLogs(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> LoginLogs(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> LoginLog(e));
        }
        public static IDictionary<string, object> LoginLog(IDictionary<string, object> data, bool isCommission=false, bool isCredit=false){
            dynamic o = Dynamic.Object;
            o.DateDisplay = data["RGS_TRN_TS_NM"].Str();
            o.DeviceName = data["DVC_NM"].Str();
            return o;
        }
    }
}