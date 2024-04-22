using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class SalesReportDto
    {
        public static IEnumerable<dynamic> FilterMaxSalesReports(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = MaxSalesReports(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> MaxSalesReports(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> MaxSalesReport(e));
        }
        public static IDictionary<string, object> MaxSalesReport(IDictionary<string, object> data, bool isCommission=false, bool isCredit=false){
            dynamic o = Dynamic.Object;
            DateTime DateTransaction = (DateTime)data["TRN_DT"];
            o.DateDisplay = DateTransaction.ToString("MMM dd, yyyy");
            o.Sales = data["TOT_SLS"].Str().ToDecimalDouble();
            try{
                DateTime datetime = (DateTime)data["MX_TRN_DT"];
                o.DateOfMaxSalesDisplay = datetime.ToString("MMM dd, yyyy");
                o.MaxSales = data["MX_TOT_SLS"].Str().ToDecimalDouble();
            }catch{}
            return o;
        }
    }

}