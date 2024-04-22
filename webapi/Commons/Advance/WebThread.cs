using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Web; 

namespace webapi.Commons.Advance
{
    public class WebThread : System.Timers.Timer
    {
        public bool IsStop { get; set; } = false;
        public Status Result { get; set; } = Status.Null;

        public WebThread(double timeout)
        {
            AutoReset = false;
            Interval = TimeSpan.FromMinutes(timeout).TotalMilliseconds;
            Elapsed += (sender, e) => { IsStop = true; };
            Start();
        }

        public static WebThread Run(Action<WebThread> func = null, double timeout = 0)
        {
            if (func != null && timeout != 0)
            {
                var thread = new WebThread(timeout);
                func(thread);
                thread.Result = thread.IsStop ? Status.Timeout : Status.Ok;
                return thread;
            }
            return null;
        }
        public enum Status
        {
            Null,
            Timeout,
            Ok
        }
    }
}
