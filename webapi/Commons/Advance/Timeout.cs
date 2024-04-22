using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Web; 
internal static class Timeout
{
    private static readonly ConcurrentDictionary<int, Thread> InnerDic;

    private static int _handle;

    static Timeout()
    {
        InnerDic = new ConcurrentDictionary<int, Thread>();
    }

    public static int Set(Action action, int delayMs = 0) 
    {
        var handle = Interlocked.Increment(ref _handle);
        var thread = new Thread(new ThreadStart(delegate
        {
            if(delayMs>0) Thread.Sleep(delayMs);
            Thread _ = null;
            InnerDic.TryRemove(handle, out _);
            if(_ == null) return;
            Task.Factory.StartNew(action);
        }));
        InnerDic.TryAdd(handle, thread);

        thread.Start();
        return handle;
    }

    public static void Clear(int handle)
    {
        Thread thread = null;
        if (InnerDic.TryRemove(handle, out thread)){
            //if(thread!=null) 
            //    thread.Abort(); //System.PlatformNotSupportedException: Thread abort is not supported on this platform.
        }
    }
}