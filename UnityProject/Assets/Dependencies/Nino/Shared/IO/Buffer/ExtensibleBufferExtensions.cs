using System;
using System.IO;

namespace Nino.Shared.IO
{
    /// <summary>
    /// Extensible buffer ext
    /// </summary>
    public static class ExtensibleBufferExtensions
    {
        /// <summary>
        /// Write data to stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        public static void WriteToStream(this ExtensibleBuffer<byte> buffer, Stream stream, int length)
        {
            byte[] bytes = ArrayPool<byte>.Request(4096);
            if (length <= 4096)
            {
                buffer.CopyTo(ref bytes, 0, length);
                stream.Write(bytes, 0, length);
                ArrayPool<byte>.Return(bytes);
                return;
            }

            int wrote = 0;
            while(length > 0)
            {
                int sizeToWrite = length <= 4096 ? length : 4096;
                buffer.CopyTo(ref bytes, wrote, sizeToWrite);
                stream.Write(bytes, 0, sizeToWrite);
                length -= sizeToWrite;
                wrote += sizeToWrite;
            }
            ArrayPool<byte>.Return(bytes);
        }
        
        /// <summary>
        /// Write data to stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        // ReSharper disable RedundantNameQualifier
        public static unsafe void WriteToStream(this ExtensibleBuffer<byte> buffer, Nino.Shared.IO.DeflateStream stream, int length)
            // ReSharper restore RedundantNameQualifier
        {
            stream.Write(buffer.Data, 0, length);
        }

#if !NETSTANDARD && !NET461 && !UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Write data to stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        public static unsafe void WriteToStream(this ExtensibleBuffer<byte> buffer, System.IO.Compression.DeflateStream stream, int length)
        {
            stream.Write(new ReadOnlySpan<byte>(buffer.Data,length));
        }
#endif
    }
}