using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Nino.Shared.IO
{
    /// <summary>
    /// A buffer that can dynamically extend
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed unsafe class ExtensibleBuffer<T> where T : unmanaged
    {
        /// <summary>
        /// Default size of the buffer
        /// </summary>
        private const int DefaultBufferSize = 128;
        
        /// <summary>
        /// Data that stores everything
        /// </summary>
        public T* Data { get; private set; }

        /// <summary>
        /// Size of T
        /// </summary>
        private readonly byte sizeOfT;
        
        /// <summary>
        /// expand size for each block
        /// </summary>
        public readonly int ExpandSize;

        /// <summary>
        /// Total length of the buffer
        /// </summary>
        public int TotalLength { get; private set; }

        /// <summary>
        /// Init buffer
        /// </summary>
        public ExtensibleBuffer() : this(DefaultBufferSize)
        {

        }
        
        /// <summary>
        /// Init extensible buffer with a capacity
        /// </summary>
        /// <param name="size"></param>
        public ExtensibleBuffer(int size = DefaultBufferSize)
        {
            sizeOfT = (byte)sizeof(T);
            ExpandSize = size;
            Data = (T*)Marshal.AllocHGlobal(sizeOfT * ExpandSize);
            TotalLength = ExpandSize;
            GC.AddMemoryPressure(sizeOfT * ExpandSize);
        }

        /// <summary>
        /// Get element at index
        /// </summary>
        /// <param name="index"></param>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                EnsureCapacity(ref index);
                Data[index] = value;
            }
        }

        /// <summary>
        /// Ensure index exists
        /// </summary>
        /// <param name="index"></param>
        private void EnsureCapacity(ref int index)
        {
            if (index < TotalLength) return;
            while (index >= TotalLength)
            {
                TotalLength += ExpandSize;
                GC.AddMemoryPressure(sizeOfT * ExpandSize);
            }
            Extend();
        }

        /// <summary>
        /// Extend buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Extend()
        {
            Data = (T*)Marshal.ReAllocHGlobal((IntPtr)Data, new IntPtr(TotalLength * sizeOfT));
        }

        /// <summary>
        /// Convert buffer data to an Array (will create a new array and copy values)
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public T[] ToArray(int startIndex, int length)
        {
            T[] ret = new T[length];
            CopyTo(ref ret, startIndex, length);
            return ret;
        }

        /// <summary>
        /// convert an extensible to buffer from start index with provided length
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Span<T> AsSpan(int startIndex, int length)
        {
            var l = startIndex + length;
            //size check
            EnsureCapacity(ref l);
            return new Span<T>(Data + startIndex, length);
        }

        /// <summary>
        /// Convert to span
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static implicit operator Span<T>(ExtensibleBuffer<T> buffer) => buffer.AsSpan(0, buffer.TotalLength);

        /// <summary>
        /// Copy data to extensible buffer
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcIndex"></param>
        /// <param name="dstIndex"></param>
        /// <param name="length"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void CopyFrom(T[] src, int srcIndex, int dstIndex, int length)
        {
            fixed (T* ptr = src)
            {
                CopyFrom(ptr, srcIndex, dstIndex, length);
            }
        }

        /// <summary>
        /// Copy data to extensible buffer
        /// why unaligned? https://stackoverflow.com/a/72418388
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcIndex"></param>
        /// <param name="dstIndex"></param>
        /// <param name="length"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void CopyFrom(T* src, int srcIndex, int dstIndex, int length)
        {
            var l = dstIndex + length;
            //size check
            EnsureCapacity(ref l);
            //copy
            Unsafe.CopyBlockUnaligned(Data + dstIndex, src + srcIndex, (uint)length);
        }

        /// <summary>
        /// Copy data from buffer to dst from dst[0]
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="srcIndex"></param>
        /// <param name="length"></param>
        /// <exception cref="OverflowException"></exception>
        public void CopyTo(ref T[] dst, int srcIndex, int length)
        {
            fixed (T* ptr = dst)
            {
                CopyTo(ptr, srcIndex, length);
            }
        }

        /// <summary>
        /// Copy data from buffer to dst from dst[0]
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="srcIndex"></param>
        /// <param name="length"></param>
        /// <exception cref="OverflowException"></exception>
        public void CopyTo(T* dst, int srcIndex, int length)
        {
            var l = srcIndex + length;
            //size check
            EnsureCapacity(ref l);
            //copy
            Unsafe.CopyBlockUnaligned(dst, Data + srcIndex, (uint)length);
        }
        
        /// <summary>
        /// Free allocated memories
        /// </summary>
        ~ExtensibleBuffer()
        {
            Marshal.FreeHGlobal((IntPtr)Data);
            GC.RemoveMemoryPressure(sizeOfT * TotalLength);
        }
    }
}