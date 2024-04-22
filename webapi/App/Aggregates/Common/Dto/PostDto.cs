using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class PostDto
    {
        public static IEnumerable<dynamic> FilterAnnouncements(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = Announcements(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> Announcements(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> Announcement(e));
        }
        public static IDictionary<string, object> Announcement(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.SequenceNo = data["SEQ_NO"].Str();
            o.Title = data["MSG_TTLE"];
            o.MessageBody = data["MSG_BODY"];
            o.DateTransaction = data["RGS_TRN_TS"];
            /*try{
                DateTime DateTransaction = (DateTime)data["RGS_TRN_TS"];
                o.TimeDisplay = DateTransaction.ToString("MMM dd, yyyy");
                o.DateDisplay = DateTransaction.ToString("hh:mm:ss tt");
            }catch{}*/
            return o;
        }

        public static IEnumerable<dynamic> FilterHelpCenters(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = HelpCenters(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.SequenceNo;
            }
            return items;
        }
        public static IEnumerable<dynamic> HelpCenters(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> HelpCenter(e));
        }

        public static IDictionary<string, object> HelpCenter(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.SequenceNo = data["SEQ_NO"].Str();
            o.Title = data["MSG_TTLE"];
            o.MessageBody = data["MSG_BODY"];
            o.DateTransaction = data["RGS_TRN_TS"];
            /*try{
                DateTime DateTransaction = (DateTime)data["RGS_TRN_TS"];
                o.TimeDisplay = DateTransaction.ToString("MMM dd, yyyy");
                o.DateDisplay = DateTransaction.ToString("hh:mm:ss tt");
            }catch{}*/
            return o;
        }
    }
}