namespace Comm.Commons.Extensions
{
    public static class ArrayExt
    {
        public static void Clear(this byte[] bytes)
        {
            if(bytes==null)return;
            bytes = new byte[bytes.Length];
            bytes = new byte[0];
            bytes = null;
        }
    }
}