using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Comm.Commons.Extensions;

namespace webapi.App.Aggregates.Common.Dto
{
    public class TickectingSubscriberDto
    {
        public static IDictionary<string, object> GetSubscriberList(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            o.PL_ID = data["PL_ID"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetDepartmentList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetDepartment_ist(data);
            return items;
        }
        public static IEnumerable<dynamic> GetDepartment_ist(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_Department_ist(e));
        }
        public static IDictionary<string, object> Get_Department_ist(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.DepartmentID = data["DEPTID"].Str();
            o.DepartmentName = data["DEPT_DESCR"].Str();
            return o;
        }
    }
}
