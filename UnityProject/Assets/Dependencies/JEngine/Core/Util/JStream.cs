using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace JEngine.Core
{
    public unsafe class JStream : Stream
    {
        private byte* _buffer; // Either allocated internally or externally.
        private readonly int _origin; // For user-provided arrays, start at this origin
        private int _position; // read/write head.
        private int _length; // Number of bytes within the memory stream
        private int _capacity; // length of usable portion of buffer for stream
        private byte[] _key; //解密密码
        private string _defaultKey = "hello_JEngine_!_";

        private bool _encrypted = true; //是否aes加密了
        private bool _isOpen; // Is this stream open or closed?

        private const int MemStreamMaxLength = Int32.MaxValue;

        public bool Encrypted
        {
            get => _encrypted;
            set => _encrypted = value;
        }

        public JStream(byte[] buffer, string key)
        {
            _buffer = (byte*)UnsafeUtility.Malloc(buffer.Length, 1, Allocator.Persistent);
            buffer.AsSpan().CopyTo(new Span<byte>(_buffer, buffer.Length));
            _length = _capacity = buffer.Length;

            _origin = 0;
            _isOpen = true;
            if (key.Length < 16)
            {
                key = InitJEngine.Instance.key.Length < 16 ? _defaultKey : InitJEngine.Instance.key;
            }

            _key = Encoding.UTF8.GetBytes(key);
            Xor();
        }

        private void Xor()
        {
            var cnt = _key.Length;
            var i = 0;
            fixed (byte* ptr = _key)
            {
                while(i < cnt)
                {
                    *(ptr + i) = (byte)(*(ptr + i) ^ i);
                    i++;
                }
            }
        }

        public override bool CanRead => _isOpen;

        public override bool CanSeek => _isOpen;

        public override bool CanWrite => false;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _isOpen = false;
                    UnsafeUtility.Free(_buffer, Allocator.Persistent);
                    _buffer = null;
                }
            }
            finally
            {
                // Call base.Close() to cleanup async IO resources
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
        }

        // Gets & sets the capacity (number of bytes allocated) for this stream.
        // The capacity cannot be set to a value less than the current length
        // of the stream.
        // 
        public virtual int Capacity
        {
            get
            {
                if (!_isOpen) Log.PrintError("stream is closed");
                return _capacity - _origin;
            }
        }

        public override long Length
        {
            get
            {
                if (!_isOpen) Log.PrintError("stream is closed");
                return _length - _origin;
            }
        }

        public override long Position
        {
            get
            {
                if (!_isOpen) Log.PrintError("stream is closed");
                return _position - _origin;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "value < 0 is invalid");
                if (!_isOpen) Log.PrintError("stream is closed");

                if (value > MemStreamMaxLength)
                    throw new ArgumentOutOfRangeException(nameof(value), "value > stream length is invalid");
                _position = _origin + (int)value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int n = _length - _position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            /*
             * JEngine的分块解密
             * 理论上，aes是 每16字节为单位 加密
             * 所以只需给buffer以16位单位切割即可
             */
            if (_encrypted)
            {
                try
                {
                    Xor();
                    fixed (byte* ptr = &buffer[offset])
                    {
                        GetBytesAt(in _position, in count, in ptr);
                    }
                }
                catch (Exception ex)
                {
                    Log.PrintError(ex);
                    throw;
                }
                finally
                {
                    Xor();
                }
            }
            else
            {
                //没加密的直接读就好
                Unsafe.CopyBlockUnaligned(ref buffer[offset], ref _buffer[_position], (uint)n);
            }

            _position += n;

            return n;
        }

        /// <summary>
        /// 获取特定位置的真实数据（包含解密过程）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        private void GetBytesAt(in int start, in int length, in byte* ret)
        {
            int offset = start >> 4 << 4; // 偏移值，截取开始的地方，比如 67 变 64，相当于start - start % 16
            int count = 32 + length >> 4 << 4; //获得需要切割的数组的长度，比如 77 变 96(80+16)，77+ （32- 77/16的余数）
            //= 77 + （32-13） = 77 + 19 = 96，多16位确保不丢东西，相当于length +(32 - length % 16);

            //现在需要将buffer切割，从offset开始，到count为止
            var encryptedData = ArrayPool<byte>.Shared.Rent(count); //创建加密数据数组
            Array.Clear(encryptedData, 0, count);
            var l = _length - offset;
            if (count > l) count = l;
            Unsafe.CopyBlockUnaligned(ref encryptedData[0], ref _buffer[offset], (uint)count); //从原始数据里分割出来

            //给encryptedData解密
            var decrypt = CryptoMgr.AesDecrypt(encryptedData, _key, 0, count, CipherMode.ECB, PaddingMode.None);
            //截取decrypt，从remainder开始，到length为止，比如余数是3，那么从3-1的元素开始
            offset = start ^ offset; //相当于start % 16

            //返还借的数组
            ArrayPool<byte>.Shared.Return(encryptedData);

            //这里有个问题，比如decrypt有16字节，而result是12字节，offset是8，那么12+8 > 16，就会出现错误
            //所以这里要改一下
            // var total = offset + length;
            // if (total > decrypt.Length)
            // {
            //     Unsafe.CopyBlockUnaligned(ref ret[0], ref decrypt[offset], (uint)(decrypt.Length));
            // }
            //直接操作指针，可以略过边界检查
            Unsafe.CopyBlockUnaligned(ref ret[0], ref decrypt[offset], (uint)length);
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            if (!_isOpen) Log.PrintError("stream is closed");

            if (offset > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException(nameof(offset), "offset > stream length is invalid");
            switch (loc)
            {
                case SeekOrigin.Begin:
                {
                    int tempPosition = unchecked(_origin + (int)offset);
                    if (offset < 0 || tempPosition < _origin)
                        throw new IOException("offset < 0 from the beginning of stream is invalid");
                    _position = tempPosition;
                    break;
                }
                case SeekOrigin.Current:
                {
                    int tempPosition = unchecked(_position + (int)offset);
                    if (unchecked(_position + offset) < _origin || tempPosition < _origin)
                        throw new IOException("offset is before the stream which is invalid");
                    _position = tempPosition;
                    break;
                }
                case SeekOrigin.End:
                {
                    int tempPosition = unchecked(_length + (int)offset);
                    if (unchecked(_length + offset) < _origin || tempPosition < _origin)
                        throw new IOException("offset is before the stream which is invalid");
                    _position = tempPosition;
                    break;
                }
                default:
                    throw new ArgumentException("invalid seek origin");
            }

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("JStream does not support write method!");
        }
    }
}