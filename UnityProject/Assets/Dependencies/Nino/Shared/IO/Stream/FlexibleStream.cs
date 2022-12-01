using System;
using System.IO;
using Nino.Shared.Mgr;
using Nino.Shared.Util;

namespace Nino.Shared.IO
{
    /// <summary>
    /// Can change the buffer in anytime
    /// This stream provides no GC write and read method, require to use it with stackalloc
    /// </summary>
    public sealed class FlexibleStream : Stream
    {
        private byte[] internalBuffer; // Either allocated internally or externally.
        private int origin; // For user-provided arrays, start at this origin
        private int position; // read/write head.
        private int length; // Number of bytes within the memory stream
        private int capacity; // length of usable portion of buffer for stream

        // Note that _capacity == _buffer.Length for non-user-provided byte[]'s

        private bool expandable; // User-provided buffers aren't expandable.
        private readonly bool exposable; // Whether the array can be returned to the user.
        private bool isOpen; // Is this stream open or closed?

        private readonly uint maxLength = 2147483648;
        private const int MemStreamMaxLength = Int32.MaxValue;

        public void ChangeBuffer(byte[] data, int offset, int count)
        {
            Reset();
            internalBuffer = data;
            origin = offset;
            position = offset;
            length = data.Length;
            capacity = length;
        }

        public void ChangeBuffer(ArraySegment<byte> data)
        {
            ChangeBuffer(data.Array, data.Offset, data.Count);
        }

        public void Reset()
        {
            position = 0;
            origin = 0;
            length = 0;
            isOpen = true;
            expandable = true;
        }

        public FlexibleStream(): this(ConstMgr.Null)
        {
            //for object pool to be able to call
        }

        public FlexibleStream(byte[] internalBuffer)
        {
            this.internalBuffer = internalBuffer ?? throw new ArgumentNullException(nameof(internalBuffer), "buffer == null");
            length = capacity = internalBuffer.Length;
            exposable = true;
            expandable = true;
            isOpen = true;
            origin = 0;
        }

        public FlexibleStream(ArraySegment<byte> internalBuffer)
        {
            this.internalBuffer = internalBuffer.Array ?? throw new ArgumentNullException(nameof(internalBuffer), "buffer == null");
            length = capacity = internalBuffer.Array.Length;
            exposable = true;
            expandable = true;
            isOpen = true;
            origin = internalBuffer.Offset;
            position = internalBuffer.Offset;
        }
        
        public override bool CanRead => isOpen;

        public override bool CanSeek => isOpen;

        public override bool CanWrite => isOpen;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    isOpen = false;
                    expandable = false;
                    // Don't set buffer to null - allow TryGetBuffer, GetBuffer & ToArray to work.
                }
            }
            finally
            {
                // Call base.Close() to cleanup async IO resources
                base.Dispose(disposing);
            }
        }

        // returns a bool saying whether we allocated a new array.
        private bool EnsureCapacity(int value)
        {
            // Check for overflow
            if (value < 0)
                throw new IOException("Stream too long, value < capacity of stream is invalid");
            if (value > capacity)
            {
                int newCapacity = value;
                if (newCapacity < 256)
                    newCapacity = 256;
                // We are ok with this overflowing since the next statement will deal
                // with the cases where _capacity*2 overflows.
                if (newCapacity < capacity * 2)
                    newCapacity = capacity * 2;
                // We want to expand the array up to Array.MaxArrayLengthOneDimensional
                // And we want to give the user the value that they asked for
                if ((uint)(capacity * 2) > maxLength)
                    newCapacity = value < maxLength ? value : (int)(maxLength / 2);

                Capacity = newCapacity;
                return true;
            }

            return false;
        }

        public override void Flush()
        {
        }

        /// <summary>
        /// Get original buffer
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public byte[] GetBuffer()
        {
            if (!exposable)
                throw new UnauthorizedAccessException("UnauthorizedAccess to get member buffer");
            return internalBuffer;
        }

        /// <summary>
        /// Try get original buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool TryGetBuffer(out ArraySegment<byte> buffer)
        {
            if (!exposable)
            {
                buffer = default(ArraySegment<byte>);
                return false;
            }

            buffer = new ArraySegment<byte>(this.internalBuffer, offset: origin, count: (length - origin));
            return true;
        }

        // Gets & sets the capacity (number of bytes allocated) for this stream.
        // The capacity cannot be set to a value less than the current length
        // of the stream.
        // 
        public int Capacity
        {
            get
            {
                if (!isOpen) Logger.E("stream is closed");
                return capacity - origin;
            }
            set
            {
                // Only update the capacity if the MS is expandable and the value is different than the current capacity.
                // Special behavior if the MS isn't expandable: we don't throw if value is the same as the current capacity
                if (value < Length)
                    throw new ArgumentOutOfRangeException(nameof(value), "value < capcacity is invalid");

                if (!isOpen) Logger.E("stream is closed");
                if (!expandable && (value != Capacity)) Logger.E("FlexibleStream is not expandable");

                // FlexibleStream has this invariant: _origin > 0 => !expandable (see ctors)
                if (expandable && value != capacity)
                {
                    if (value > 0)
                    {
                        byte[] newBuffer = new byte[value];
                        if (length > 0) Buffer.BlockCopy(internalBuffer, 0, newBuffer, 0, length);
                        internalBuffer = newBuffer;
                    }
                    else
                    {
                        internalBuffer = null;
                    }

                    capacity = value;
                }
            }
        }

        /// <summary>
        /// Length of written buffer or the buffer provided
        /// </summary>
        public override long Length
        {
            get
            {
                if (!isOpen) Logger.E("stream is closed");
                return length - origin;
            }
        }

        /// <summary>
        /// Position of the current stream
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override long Position
        {
            get
            {
                if (!isOpen) Logger.E("stream is closed");
                return position - origin;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "value < 0 is invalid");
                if (!isOpen) Logger.E("stream is closed");

                if (value > MemStreamMaxLength)
                    throw new ArgumentOutOfRangeException(nameof(value), "value > stream length is invalid");
                position = origin + (int)value;
            }
        }

        /// <summary>
        /// Read a byte from current position
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            if (!isOpen) Logger.E("stream is closed");

            if (position >= length) return -1;

            return internalBuffer[position++];
        }

        /// <summary>
        /// Read and copy some bytes to the provided buffer - has gc if buffer is a new byte[count] or byte[moreThanCount]
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "buffer == null");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "offset < 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count < 0");
            if (buffer.Length - offset < count)
                throw new ArgumentException("invalid buffer length");

            if (!isOpen) Logger.E("stream is closed");

            int n = length - position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            if (n <= 8)
            {
                int byteCount = n;
                while (--byteCount >= 0)
                    buffer[offset + byteCount] = this.internalBuffer[position + byteCount];
            }
            else
                Buffer.BlockCopy(this.internalBuffer, position, buffer, offset, n);

            position += n;

            return n;
        }

        /// <summary>
        /// Read and copy some bytes to a byte array pointer - has no gc when buffer is stackalloc byte[count]
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public unsafe int Read(byte* buffer, int offset, int count)
        {
            if (!isOpen) Logger.E("stream is closed");

            int n = length - position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            fixed (byte* internalPtr = internalBuffer)
            {
               Buffer.MemoryCopy(internalPtr + position, buffer + offset, count,n);
            }
            position += n;

            return n;
        }
        
        /// <summary>
        /// Write a byte to the stream
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public override void WriteByte(byte value) {
            if (!isOpen)
                throw new InvalidOperationException("this stream is closed");
            
            if (position >= length) {
                int newLength = position + 1;
                bool mustZero = position > length;
                if (newLength >= capacity) {
                    bool allocatedNewArray = EnsureCapacity(newLength);
                    if (allocatedNewArray)
                        mustZero = false;
                }
                if (mustZero)
                    Array.Clear(internalBuffer, length, position - length);
                length = newLength;
            }
            internalBuffer[position++] = value;
        }
        
        /// <summary>
        /// Write some bytes to the stream from a byte[count] or byte[moreThanCount]
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="IOException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer==null)
                throw new ArgumentNullException(nameof(buffer),"null");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count),"cannot be negative");
            if (buffer.Length - offset < count)
                throw new ArgumentException("invalid length");

            if (!isOpen)
                throw new InvalidOperationException("this stream is closed");
 
            int i = position + count;
            // Check for overflow
            if (i < 0)
                throw new IOException("Stream Too Long");
 
            if (i > length) {
                bool mustZero = position > length;
                if (i > capacity) {
                    bool allocatedNewArray = EnsureCapacity(i);
                    if (allocatedNewArray)
                        mustZero = false;
                }
                if (mustZero)
                    Array.Clear(internalBuffer, length, i - length);
                length = i;
            }
            if ((count <= 8) && (buffer != internalBuffer))
            {
                int byteCount = count;
                while (--byteCount >= 0)
                    internalBuffer[position + byteCount] = buffer[offset + byteCount];
            }
            else
                Buffer.BlockCopy(buffer, offset, internalBuffer, position, count);
            position = i;
        }
        
        /// <summary>
        /// Write some bytes to the stream from a stackalloc byte[count]
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="IOException"></exception>
        public unsafe void Write(byte* buffer, int offset, int count)
        {
            if (buffer==null)
                throw new ArgumentNullException(nameof(buffer),"null");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count),"cannot be negative");

            if (!isOpen)
                throw new InvalidOperationException("this stream is closed");
 
            int i = position + count;
            // Check for overflow
            if (i < 0)
                throw new IOException("Stream Too Long");
 
            if (i > length) {
                bool mustZero = position > length;
                if (i > capacity) {
                    bool allocatedNewArray = EnsureCapacity(i);
                    if (allocatedNewArray)
                        mustZero = false;
                }
                if (mustZero)
                    Array.Clear(internalBuffer, length, i - length);
                length = i;
            }
            
            fixed (byte* internalPtr = internalBuffer)
            {
                Buffer.MemoryCopy(buffer + offset, internalPtr + position, count, count);
            }
            
            position = i;
        }

        /// <summary>
        /// Seek the stream
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public override long Seek(long offset, SeekOrigin loc)
        {
            if (!isOpen) Logger.E("stream is closed");

            if (offset > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException(nameof(offset), "offset > stream length is invalid");
            switch (loc)
            {
                case SeekOrigin.Begin:
                {
                    int tempPosition = unchecked(origin + (int)offset);
                    if (offset < 0 || tempPosition < origin)
                        throw new IOException("offset < 0 from the beginning of stream is invalid");
                    position = tempPosition;
                    break;
                }
                case SeekOrigin.Current:
                {
                    int tempPosition = unchecked(position + (int)offset);
                    if (unchecked(position + offset) < origin || tempPosition < origin)
                        throw new IOException("offset is before the stream which is invalid");
                    position = tempPosition;
                    break;
                }
                case SeekOrigin.End:
                {
                    int tempPosition = unchecked(length + (int)offset);
                    if (unchecked(length + offset) < origin || tempPosition < origin)
                        throw new IOException("offset is before the stream which is invalid");
                    position = tempPosition;
                    break;
                }
                default:
                    throw new ArgumentException("invalid seek origin");
            }

            return position;
        }

        // Sets the length of the stream to a given value.  The new
        // value must be nonnegative and less than the space remaining in
        // the array, Int32.MaxValue - origin
        // Origin is 0 in all cases other than a FlexibleStream created on
        // top of an existing array and a specific starting offset was passed 
        // into the FlexibleStream constructor.  The upper bounds prevents any 
        // situations where a stream may be created on top of an array then 
        // the stream is made longer than the maximum possible length of the 
        // array (Int32.MaxValue).
        // 
        public override void SetLength(long value)
        {
            if (value < 0 || value > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value does not fit the length (out of range)");
            }

            // Origin wasn't publicly exposed above.
            if (value > (Int32.MaxValue - origin))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value is too big");
            }

            int newLength = origin + (int)value;
            bool allocatedNewArray = EnsureCapacity(newLength);
            if (!allocatedNewArray && newLength > length)
                Array.Clear(internalBuffer, length, newLength - length);
            length = newLength;
            if (position > newLength) position = newLength;

        }

        /// <summary>
        /// Get written bytes to array - will cause GC
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            byte[] copy = new byte[length - origin];
            Buffer.BlockCopy(internalBuffer, origin, copy, 0, length - origin);
            return copy;
        }
    }
}