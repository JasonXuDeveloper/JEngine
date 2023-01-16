using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace JEngine.Core
{
#if INIT_JE
    /// <summary>
    /// Memory pool that returns unmanaged memory
    /// </summary>
    public unsafe class UnmanagedMemoryPool
    {
        /// <summary>
        /// Allocated unmanaged memory
        /// Structure:
        /// Chain:
        /// 1 byte of whether or not hold in use -> 8 bytes of length of n -> n
        /// ---- ---- ---- ---- ----  ----
        /// | 1 | 8 |  n1 |  1 |  8  | n2 | ......
        /// ---- ---- ---- ---- ----  ----
        /// </summary>
        private readonly byte* _memory;

        /// <summary>
        /// Allocated size
        /// </summary>
        private readonly long _memoryLength;
        
        /// <summary>
        /// Create a memory pool
        /// </summary>
        /// <param name="size"></param>
        public UnmanagedMemoryPool(long size = 1024)
        {
            _memory = (byte*)UnsafeUtility.Malloc(size, 1, Allocator.Persistent);
            //ensure 0
            UnsafeUtility.MemClear(_memory, size);
            *_memory = 0;
            _memoryLength = size;
            GC.AddMemoryPressure(size);
        }
        
        /// <summary>
        /// Destroy the memory pool
        /// </summary>
        ~UnmanagedMemoryPool()
        {
            Destroy();
        }
        
        /// <summary>
        /// Destroy the current memory pool
        /// </summary>
        public void Destroy()
        {
            UnsafeUtility.Free(_memory, Allocator.Persistent);
            GC.RemoveMemoryPressure(_memoryLength);
        }

        /// <summary>
        /// Allocate memory
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="OutOfMemoryException"></exception>
        public byte* Allocate(long size)
        {
            //look for valid blocks
            byte* ptr = _memory;
            while (true)
            {
                //ensure it is not out of boundary
                if (ptr - _memory > _memoryLength || ptr + 9 - _memory > _memoryLength)
                {
                    //no enough allocated memory
                    throw new OutOfMemoryException("memory pool out of memory");
                }

                if (*ptr == 0)
                {
                    //if the block is not in use, check if the size is enough
                    long len = *(long*)(ptr + 1);
                    byte* next = ptr + 9 + len;
                    //ensure won't cross the boundary
                    if (next > _memory + _memoryLength)
                    {
                        //no enough allocated memory
                        throw new OutOfMemoryException("memory pool out of memory");
                    }
                    //ensure not intersecting with the next block
                    if (*next == 0)
                    {
                        if (len == 0 || len >= size)//0 -> uninitialized
                        {
                            //if the size is enough, mark the block as in use
                            *ptr = 1;
                            //mark size
                            *(long*)(ptr + 1) = size;
                            return ptr + 9;
                        }
                    }
                }

                //if the block is in use, or the size is not enough, move to the next block
                ptr += *(long*)(ptr + 1) + 9;
            }
        }
        
        /// <summary>
        /// Free allocated memory
        /// </summary>
        /// <param name="ptr"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Free(byte* ptr)
        {
            //ensure p is inside of the memory pool
            if (ptr < _memory || ptr >= _memory + _memoryLength)
            {
                throw new ArgumentException("ptr is not inside of the memory pool");
            }
            //get length of p
            long len = *(long*)(ptr - 8);
            //clear
            UnsafeUtility.MemClear(ptr, len);
            //mark the block as not in use
            *(ptr - 9) = 0;
        }
    }
#endif
}