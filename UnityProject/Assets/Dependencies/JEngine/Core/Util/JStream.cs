using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace JEngine.Core
{
    public sealed class JStream : Stream
    {
        private const string DefaultKey = "hello_JEngine_!_";
        private const int MemStreamMaxLength = Int32.MaxValue;

        private byte[] _buffer; // Either allocated internally or externally.
        private readonly int _origin; // For user-provided arrays, start at this origin
        private int _position; // read/write head.
        private readonly int _length; // Number of bytes within the memory stream
        private readonly int _capacity; // length of usable portion of buffer for stream
        private byte[] _decryptorBuffer; // Use for decryptor
        private readonly ICryptoTransform _decryptor;

        private bool _encrypted = true; //是否aes加密了
        private bool _isOpen; // Is this stream open or closed?

        public bool Encrypted
        {
            get => _encrypted;
            set => _encrypted = value;
        }

        public JStream(byte[] buffer, string key)
        {
            _buffer = new byte[buffer.Length];
            Unsafe.CopyBlockUnaligned(ref _buffer[0], ref buffer[0], (uint)buffer.Length);
            _length = _capacity = buffer.Length;

            _origin = 0;
            _isOpen = true;
            if (key.Length < 16)
            {
                key = InitJEngine.Instance.key.Length < 16 ? DefaultKey : InitJEngine.Instance.key;
            }

            var algo = Aes.Create();
            algo.Key = Encoding.UTF8.GetBytes(key);
            algo.Mode = CipherMode.ECB;
            algo.Padding = PaddingMode.None;
            _decryptor = algo.CreateDecryptor();

            _decryptorBuffer = new byte[64 * 1024]; // 64kb buffer for decryptor
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
                    _buffer = null;
                    _decryptorBuffer = null;
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
        public int Capacity
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

            if (_encrypted)
            {
                //JEngine的分块解密
                GetBytesAt(_position, count, buffer, offset);
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
        /// <param name="retOffset"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBytesAt(int start, int length, byte[] ret, int retOffset)
        {
            int offset = start & 0x7ffffff0; // 偏移值，截取开始的地方，比如 67 变 64，相当于start - start % 16
            int count = 32 + length & 0x7ffffff0; //获得需要切割的数组的长度，比如 77 变 96(80+16)，77+ （32- 77%16）
            //= 77 + （32-13） = 77 + 19 = 96，多16位确保不丢东西，相当于length +(32 - length % 16);

            if (_decryptorBuffer.Length < count)
            {
                Array.Resize(ref _decryptorBuffer, count);
            }

            //解密
            int decryptedBytes = _decryptor.TransformBlock(_buffer, offset, count, _decryptorBuffer, 0);

            //截取decrypt，从remainder开始，到length为止，比如余数是3，那么从3-1的元素开始
            offset = start ^ offset; //相当于start % 16

            //直接操作指针，可以略过边界检查
            Unsafe.CopyBlockUnaligned(ref ret[retOffset], ref _decryptorBuffer[offset],
                (uint)Math.Min(decryptedBytes, length));
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