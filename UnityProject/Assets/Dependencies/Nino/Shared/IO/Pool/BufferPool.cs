using System;

namespace Nino.Shared.IO
{
    /// <summary>
    /// Thread safe byte array (buffer) pool
    /// </summary>
    public static class BufferPool
    {
        /// <summary>
        /// A shared buffer queue
        /// </summary>
        private static readonly UncheckedStack<byte[]> Buffers = new UncheckedStack<byte[]>(3);

        /// <summary>
        /// lock obj
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        /// Request a buffer
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] RequestBuffer(int size = 0)
        {
            lock (Lock)
            {
                byte[] ret;
                if (Buffers.Count > 0)
                {
                    ret = Buffers.Pop();
                    if (ret.Length < size)
                    {
                        byte[] buffer = new byte[size];
                        ReturnBuffer(ret);
                        return buffer;
                    }
                }
                else
                {
                    ret = new byte[size];
                }

                return ret;
            }
        }

        /// <summary>
        /// Preview next cache buffer's length
        /// </summary>
        /// <returns></returns>
        public static int PreviewNextCacheBufferLength()
        {
            lock (Lock)
            {
                if (Buffers.Count == 0) return 0;
                return Buffers.Peek().Length;
            }
        }
        
        /// <summary>
        /// Request a buffer from a source
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static byte[] RequestBuffer(byte[] original)
        {
            byte[] ret = RequestBuffer(original.Length);
            Buffer.BlockCopy(original,0,ret,0,original.Length);
            return ret;
        }
        
        /// <summary>
        /// Request a buffer from a source
        /// </summary>
        /// <param name="len"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        public static byte[] RequestBuffer(int len, byte[] original)
        {
            byte[] ret = RequestBuffer(len);
            Buffer.BlockCopy(original,0,ret,0,original.Length);
            return ret;
        }

        /// <summary>
        /// Return buffer to the pool
        /// </summary>
        /// <param name="buffer"></param>
        public static void ReturnBuffer(byte[] buffer)
        {
            lock (Lock)
            {
                Buffers.Push(buffer);
            }
        }
    }
}