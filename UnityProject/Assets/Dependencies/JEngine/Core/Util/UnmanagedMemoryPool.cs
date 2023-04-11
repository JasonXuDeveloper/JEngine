//
// UnmanagedMemoryPool.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace JEngine.Core
{
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
}