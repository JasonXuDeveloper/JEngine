using System;
using System.IO;
using Nino.Shared.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using DeflateStream = Nino.Shared.IO.DeflateStream;
// ReSharper disable UnusedMember.Local
// ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162

namespace Nino.Shared.Mgr
{
    public static class CompressMgr
    {
        /// <summary>
        /// static ctor
        /// </summary>
        static CompressMgr()
        {
            if (!ConstMgr.EnableNativeDeflate) return;
            GetCompressInformation(out _, out _);
            var compressedStream = new FlexibleStream(BufferPool.RequestBuffer(10240));
            var zipStream = new DeflateStream(compressedStream, CompressionMode.Compress, true);
            CompressStreams.Push(zipStream);
            var empty = new ArraySegment<byte>(Array.Empty<byte>());
            GetDecompressInformation(out _, ref empty);
            compressedStream = new FlexibleStream(BufferPool.RequestBuffer(10240));
            zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress, true);
            DecompressStreams.Push(zipStream);
        }

        /// <summary>
        /// compress stream pool (deflateStream compress + flexibleStream)
        /// </summary>
        private static readonly UncheckedStack<DeflateStream> CompressStreams = new UncheckedStack<DeflateStream>();

        /// <summary>
        /// decompress stream pool (deflateStream decompress + flexibleStream)
        /// </summary>
        private static readonly UncheckedStack<DeflateStream> DecompressStreams = new UncheckedStack<DeflateStream>();

        /// <summary>
        /// lock compressed streams
        /// </summary>
        private static readonly object CompressedLock = new object();


        /// <summary>
        /// lock decompressed streams
        /// </summary>
        private static readonly object DecompressedLock = new object();

        /// <summary>
        /// Compress the given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] data)
        {
            return Compress(new ArraySegment<byte>(data));
        }

        /// <summary>
        /// Compress the given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Compress(ArraySegment<byte> data)
        {
            lock (CompressedLock)
            {
                if (!ConstMgr.EnableNativeDeflate)
                {
                    return CompressOnNative(data);
                }

                GetCompressInformation(out var zipStream, out var compressedStream);
                // ReSharper disable AssignNullToNotNullAttribute
                zipStream.Write(data.Array, data.Offset, data.Count);
                // ReSharper restore AssignNullToNotNullAttribute
                return GetCompressBytes(zipStream, compressedStream);
            }
        }

        /// <summary>
        /// Compress the given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] Compress(ExtensibleBuffer<byte> data, int length)
        {
            lock (CompressedLock)
            {
                if (!ConstMgr.EnableNativeDeflate)
                {
                    return CompressOnNative(data, length);
                }

                GetCompressInformation(out var zipStream, out var compressedStream);
                data.WriteToStream(zipStream, length);
                return GetCompressBytes(zipStream, compressedStream);
            }
        }


        /// <summary>
        /// Get compressed data
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="compressedStream"></param>
        /// <returns></returns>
        private static byte[] GetCompressBytes(DeflateStream zipStream, FlexibleStream compressedStream)
        {
            zipStream.Finish();
            //push
            CompressStreams.Push(zipStream);
            return compressedStream.ToArray();
        }

        /// <summary>
        /// Get relevant data
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="compressedStream"></param>
        private static void GetCompressInformation(out DeflateStream zipStream, out FlexibleStream compressedStream)
        {
            lock (CompressedLock)
            {
                //try get stream
                if (CompressStreams.Count > 0)
                {
                    zipStream = CompressStreams.Pop();
                    zipStream.Reset();
                    compressedStream = zipStream.BaseStream;
                }
                else
                {
                    //create
                    compressedStream = new FlexibleStream();
                    zipStream = new DeflateStream(compressedStream, CompressionMode.Compress, true);
                }
            }
        }

        /// <summary>
        /// Decompress thr given bytes (NEED TO BE AWARE OF UNMANAGED INTPTR, REMEMBER TO FREE IT)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputLength"></param>
        /// <returns></returns>
        public static IntPtr Decompress(byte[] data, out int outputLength)
        {
            lock (DecompressedLock)
            {
                var seg = new ArraySegment<byte>(data);
                if (!ConstMgr.EnableNativeDeflate)
                {
                    return DecompressOnNative(seg, out outputLength);
                }

                GetDecompressInformation(out var zipStream, ref seg);
                var ret = zipStream.GetDecompressedBytes(out outputLength, data.Length);
                //push
                DecompressStreams.Push(zipStream);
                return ret;
            }
        }

        /// <summary>
        /// Decompress thr given bytes (NEED TO BE AWARE OF UNMANAGED INTPTR, REMEMBER TO FREE IT)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outputLength"></param>
        /// <returns></returns>
        public static IntPtr Decompress(ArraySegment<byte> data, out int outputLength)
        {
            lock (DecompressedLock)
            {
                if (!ConstMgr.EnableNativeDeflate)
                {
                    return DecompressOnNative(data, out outputLength);
                }

                GetDecompressInformation(out var zipStream, ref data);
                var ret = zipStream.GetDecompressedBytes(out outputLength, data.Count);
                //push
                DecompressStreams.Push(zipStream);
                return ret;
            }
        }

        /// <summary>
        /// Decompress thr given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            lock (DecompressedLock)
            {
                var seg = new ArraySegment<byte>(data);
                if (!ConstMgr.EnableNativeDeflate)
                {
                    var ptr = DecompressOnNative(seg, out var length);
                    byte[] buf = new byte[length];
                    Marshal.Copy(ptr, buf, 0, length);
                    Marshal.FreeHGlobal(ptr);
                    return buf;
                }

                GetDecompressInformation(out var zipStream, ref seg);
                var ret = zipStream.GetDecompressedBytes(out var len, data.Length);
                //push
                DecompressStreams.Push(zipStream);
                var buffer = new byte[len];
                Marshal.Copy(ret, buffer, 0, len);
                Marshal.FreeHGlobal(ret);
                return buffer;
            }
        }

        /// <summary>
        /// Decompress thr given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Decompress(ArraySegment<byte> data)
        {
            lock (DecompressedLock)
            {
                if (!ConstMgr.EnableNativeDeflate)
                {
                    var ptr = DecompressOnNative(data, out var length);
                    byte[] buf = new byte[length];
                    Marshal.Copy(ptr, buf, 0, length);
                    Marshal.FreeHGlobal(ptr);
                    return buf;
                }

                GetDecompressInformation(out var zipStream, ref data);
                var ret = zipStream.GetDecompressedBytes(out var len, data.Count);
                //push
                DecompressStreams.Push(zipStream);
                var buffer = new byte[len];
                Marshal.Copy(ret, buffer, 0, len);
                Marshal.FreeHGlobal(ret);
                return buffer;
            }
        }


        /// <summary>
        /// Get relevant data
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="data"></param>
        private static void GetDecompressInformation(out DeflateStream zipStream, ref ArraySegment<byte> data)
        {
            lock (DecompressedLock)
            {
                //try get stream
                if (DecompressStreams.Count > 0)
                {
                    zipStream = DecompressStreams.Pop();
                    zipStream.Reset();
                    var dataStream = zipStream.BaseStream;
                    dataStream.ChangeBuffer(data);
                }
                else
                {
                    //create
                    var dataStream = new FlexibleStream(data);
                    zipStream = new DeflateStream(dataStream, CompressionMode.Decompress, true);
                }
            }
        }

        #region NON_UNITY

        /// <summary>
        /// Compress the given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] CompressOnNative(ArraySegment<byte> data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new System.IO.Compression.DeflateStream(compressedStream, CompressionMode.Compress))
            {
                // ReSharper disable AssignNullToNotNullAttribute
                zipStream.Write(data.Array, data.Offset, data.Count);
                // ReSharper restore AssignNullToNotNullAttribute
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        /// <summary>
        /// Compress the given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static byte[] CompressOnNative(ExtensibleBuffer<byte> data, int length)
        {
            using (var compressedStream = new MemoryStream(length))
            using (var zipStream = new System.IO.Compression.DeflateStream(compressedStream, CompressionMode.Compress))
            {
                data.WriteToStream(zipStream, length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress thr given bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private static IntPtr DecompressOnNative(ArraySegment<byte> data, out int len)
        {
            FlexibleStream result = ObjectPool<FlexibleStream>.Request();
            result.Reset();
            FlexibleStream compressedStream;
            if (ObjectPool<FlexibleStream>.Peak() != null)
            {
                compressedStream = ObjectPool<FlexibleStream>.Request();
                compressedStream.ChangeBuffer(data.Array, data.Offset, data.Count);
            }
            else
            {
                compressedStream = new FlexibleStream(data);
            }

            using (var zipStream =
                new System.IO.Compression.DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                zipStream.CopyTo(result);
                ObjectPool<FlexibleStream>.Return(compressedStream);
                len = (int)result.Length;
                IntPtr ptr = Marshal.AllocHGlobal(len);
                Marshal.Copy(result.GetBuffer(), 0, ptr, len);
                ObjectPool<FlexibleStream>.Return(result);
                return ptr;
            }
        }

        #endregion
    }
}