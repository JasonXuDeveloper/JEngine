using System;
using System.IO;

namespace JEngine.Core
{
    public class JStream : Stream
    {
        private byte[] _buffer;    // Either allocated internally or externally.
        private readonly int _origin;       // For user-provided arrays, start at this origin
        private int _position;     // read/write head.
        private int _length;       // Number of bytes within the memory stream
        private int _capacity;     // length of usable portion of buffer for stream
        private string _key;        //解密密码
        private string _defaultKey = "hello_JEngine_!_";
        
        #if UNITY_EDITOR
        public long EncryptedCounts { get; set; }
        #endif

        // Note that _capacity == _buffer.Length for non-user-provided byte[]'s
 
        private bool _encrypted = true;//是否aes加密了
        private bool _expandable;  // User-provided buffers aren't expandable.
        private readonly bool _exposable;   // Whether the array can be returned to the user.
        private bool _isOpen;      // Is this stream open or closed?

        private readonly uint maxLength = 2147483648;

        private const int MemStreamMaxLength = Int32.MaxValue;

        public bool Encrypted
        {
            get => _encrypted;
            set => _encrypted = value;
        }

        public JStream(byte[] buffer, string key)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer), "buffer == null");
            _length = _capacity = buffer.Length;
            _exposable = false;
            _origin = 0;
            _isOpen = true;
            _key = key;
            if (_key.Length < 16)
            {
                _key = InitJEngine.Instance.key.Length < 16 ? _defaultKey : InitJEngine.Instance.key;
            }
        }

        public JStream(byte[] buffer,string key, int index, int count) 
            : this(buffer, key, index, count, false) {
        }
    
        public JStream(byte[] buffer,string key, int index, int count, bool publiclyVisible) {
            if (buffer==null)
                throw new ArgumentNullException(nameof(buffer), "buffer == null");
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "index < 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count < 0");
            if (buffer.Length - index < count)
                throw new ArgumentException("invalid length of buffer");
            
            _buffer = buffer;
            _origin = _position = index;
            _length = _capacity = index + count;
            _exposable = publiclyVisible;  // Can TryGetBuffer/GetBuffer return the array?
            _expandable = false;
            _isOpen = true;
            _key = key;
            if (_key.Length < 16)
            {
                _key = InitJEngine.Instance.key.Length < 16 ? _defaultKey : InitJEngine.Instance.key;
            }
        }
    
        public override bool CanRead => _isOpen;

        public override bool CanSeek => _isOpen;

        public override bool CanWrite => false;

        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing) {
                    _isOpen = false;
                    _expandable = false;
                    // Don't set buffer to null - allow TryGetBuffer, GetBuffer & ToArray to work.
                }
            }
            finally {
                // Call base.Close() to cleanup async IO resources
                base.Dispose(disposing);
            }
        }
        
        // returns a bool saying whether we allocated a new array.
        private bool EnsureCapacity(int value) {
            // Check for overflow
            if (value < 0)
                throw new IOException("Stream too long, value < capacity of stream is invalid");
            if (value > _capacity) {
                int newCapacity = value;
                if (newCapacity < 256)
                    newCapacity = 256;
                // We are ok with this overflowing since the next statement will deal
                // with the cases where _capacity*2 overflows.
                if (newCapacity < _capacity * 2)
                    newCapacity = _capacity * 2;
                // We want to expand the array up to Array.MaxArrayLengthOneDimensional
                // And we want to give the user the value that they asked for
                if ((uint) (_capacity * 2) > maxLength)
                    newCapacity = value < maxLength ? value : (int)(maxLength / 2);
                
                Capacity = newCapacity;
                return true;
            }
            return false;
        }
    
        public override void Flush() {
        }
 
 
        public virtual byte[] GetBuffer() {
            if (!_exposable)
                throw new UnauthorizedAccessException("UnauthorizedAccess to get member buffer");
            return _buffer;
        }
 
        public virtual bool TryGetBuffer(out ArraySegment<byte> buffer) {
            if (!_exposable) {
                buffer = default(ArraySegment<byte>);
                return false;
            }
 
            buffer = new ArraySegment<byte>(_buffer, offset:_origin, count:(_length - _origin));
            return true;
        }
 
        // -------------- PERF: Internal functions for fast direct access of JStream buffer (cf. BinaryReader for usage) ---------------
 
        // PERF: Internal sibling of GetBuffer, always returns a buffer (cf. GetBuffer())
        internal byte[] InternalGetBuffer() {
            return _buffer;
        }
 
        // PERF: Get origin and length - used in ResourceWriter.
        internal void InternalGetOriginAndLength(out int origin, out int length)
        {
            if (!_isOpen) Log.PrintError("stream is closed");
            origin = _origin;
            length = _length;
        }
 
        // PERF: True cursor position, we don't need _origin for direct access
        internal int InternalGetPosition() {
            if (!_isOpen) Log.PrintError("stream is closed");
            return _position;
        }
 
        // PERF: Takes out Int32 as fast as possible
        internal int InternalReadInt32() {
           if (!_isOpen)
               Log.PrintError("stream is closed");
 
           int pos = (_position += 4); // use temp to avoid ----
           if (pos > _length)
           {
               _position = _length;
               Log.PrintError("end of file");
           }
           return _buffer[pos-4] | _buffer[pos-3] << 8 | _buffer[pos-2] << 16 | _buffer[pos-1] << 24;
        }
 
        // PERF: Get actual length of bytes available for read; do sanity checks; shift position - i.e. everything except actual copying bytes
        internal int InternalEmulateRead(int count) {
            if (!_isOpen)  Log.PrintError("stream is closed");
 
            int n = _length - _position;
            if (n > count) n = count;
            if (n < 0) n = 0;
 
            _position += n;
            return n;
        }
       
        // Gets & sets the capacity (number of bytes allocated) for this stream.
        // The capacity cannot be set to a value less than the current length
        // of the stream.
        // 
        public virtual int Capacity {
            get { 
                if (!_isOpen) Log.PrintError("stream is closed");
                return _capacity - _origin;
            }
            set {
                // Only update the capacity if the MS is expandable and the value is different than the current capacity.
                // Special behavior if the MS isn't expandable: we don't throw if value is the same as the current capacity
                if (value < Length) throw new ArgumentOutOfRangeException(nameof(value), "value < capcacity is invalid");
 
                if (!_isOpen)  Log.PrintError("stream is closed");
                if (!_expandable && (value != Capacity))  Log.PrintError("JStream is not expandable");
 
                // JStream has this invariant: _origin > 0 => !expandable (see ctors)
                if (_expandable && value != _capacity) {
                    if (value > 0) {
                        byte[] newBuffer = new byte[value];
                        if (_length > 0) Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _length);
                        _buffer = newBuffer;
                    }
                    else {
                        _buffer = null;
                    }
                    _capacity = value;
                }
            }
        }        
 
        public override long Length {
            get {
                if (!_isOpen) Log.PrintError("stream is closed");
                return _length - _origin;
            }
        }
 
        public override long Position {
            get { 
                if (!_isOpen) Log.PrintError("stream is closed");
                return _position - _origin;
            }
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "value < 0 is invalid");
                if (!_isOpen)  Log.PrintError("stream is closed");
 
                if (value > MemStreamMaxLength)
                    throw new ArgumentOutOfRangeException(nameof(value), "value > stream length is invalid");
                _position = _origin + (int)value;
            }
        }
 
        public override int Read(byte[] buffer, int offset, int count) {
            if (buffer==null)
                throw new ArgumentNullException(nameof(buffer), "buffer == null");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "offset < 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count < 0");
            if (buffer.Length - offset < count)
                throw new ArgumentException("invalid buffer length");

            if (!_isOpen)  Log.PrintError("stream is closed");
 
            int n = _length - _position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;
 
            /*
             MemoryStream源码是这样的，我也不知道为什么要把8个长度的单独拉出来
             if (n <= 8)
            {
                int byteCount = n;
                while (--byteCount >= 0)
                    buffer[offset + byteCount] = _buffer[_position + byteCount];
            }
            else
                Buffer.BlockCopy(_buffer, _position, buffer, offset, n);
            */
            
            /*
             * JEngine的分块解密
             * 理论上，aes是 每16字节为单位 加密
             * 所以只需给buffer以16位单位切割即可
             */
            if (_encrypted)
            {
                try
                {
                    Buffer.BlockCopy(GetBytesAt(_position, count), 0, buffer, offset, n);//复制过去
                    //这个用来做log的，去掉注释就可以调试
                    //这边的result是缓存，把log给打出来，不然分配buffer后，如果出错了没办法还原解密结果
                    // var result = new byte[buffer.Length];
                    // Buffer.BlockCopy(buffer, 0, result, 0, buffer.Length);
                    // Buffer.BlockCopy(src, 0, result, offset, n);
                    // Log.Print("解密结果：" + string.Join(", ", result));
                    // Buffer.BlockCopy(result, 0, buffer, offset, n);
                }
                catch(Exception ex)
                {
                    Log.PrintError(ex);
                    throw;
                }
            }
            else
            {
                //没加密的直接读就好
                Buffer.BlockCopy(_buffer, _position, buffer, offset, n);
                
                //对比无加密的：加密的字节，用于测试（可能会有不一样的地方）
                // Log.Print("原文结果：" + string.Join(", ", buffer));
                // var src = GetBytesAt(_position, n);
                // var result = new byte[buffer.Length];
                // Buffer.BlockCopy(buffer, 0, result, 0, buffer.Length);
                // Buffer.BlockCopy(src, 0, result, offset, n);
                // Log.Print("解密结果：" + string.Join(", ", result));
                // var equal = CompareArray(buffer,result);
                // var en = CryptoHelper.AesEncryptWithNoPadding(buffer, _key);
                // Log.Print("原文加密结果：" + string.Join(", ", en));
                // Log.Print($"pos: {_position}, count: {count}, decrypt equals original: {equal}");
                // Log.Print($"=======================");
            }
            
            _position += n;
 
            return n;
        }

        /// <summary>
        /// 数组比较是否相等
        /// </summary>
        /// <param name="bt1">数组1</param>
        /// <param name="bt2">数组2</param>
        /// <returns>true:相等，false:不相等</returns>
        public bool CompareArray(byte[] bt1, byte[] bt2)
        {
            var len1 = bt1.Length;
            var len2 = bt2.Length;
            if (len1 != len2)
            {
                return false;
            }
            for (var i = 0; i < len1; i++)
            {
                if (bt1[i] != bt2[i])
                {
                    // Log.PrintError($"original: {string.Join(",",bt1)}, decrypt: {string.Join(",",bt2)}\n" +
                                   // $"{bt1[i]} != {bt2[i]}");
                    return false;
                }
            }
            return true;
        }

        public override int ReadByte()
        {
            if (!_isOpen)  Log.PrintError("stream is closed");
            
            if (_position >= _length) return -1;

            return _encrypted ? GetBytesAt(_position++, 1)[0] : _buffer[_position++];
        }

        /// <summary>
        /// 获取特定位置的真实数据（包含解密过程）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] GetBytesAt(int start, int length)
        {
            int remainder = start % 16; // 余数
            int offset = start - remainder; // 偏移值，截取开始的地方，比如 67 变 64
            int count = length + (32 - length % 16); //获得需要切割的数组的长度，比如 77 变 96(80+16)，77+ （32- 77/16的余数） = 77 + （32-13） = 77 + 19 = 96，多16位确保不丢东西
            var result = new byte[length]; //返回值，长度为length

            // Log.Print($"start:{start}, length:{length}, remainder:{remainder}");
            
            //现在需要将buffer切割，从offset开始，到count为止
            var encryptedData = new byte[count];//创建加密数据数组
            var l = _buffer.Length - offset;
            if (count > l) count = l;
            Buffer.BlockCopy(_buffer, offset, encryptedData, 0, count);//从原始数据里分割出来

            // Log.Print("获取到的密文："+string.Join(", ", encryptedData));

            //给encryptedData解密
            var decrypt = CryptoHelper.AesDecryptWithNoPadding(encryptedData, _key);
            //截取decrypt，从remainder开始，到length为止，比如余数是3，那么从3-1的元素开始
            offset = remainder;
            
            // Log.Print($"copy from offset:{offset}, result.length:{length}, decrypt.length:{decrypt.Length}");
            
            //这里有个问题，比如decrypt有16字节，而result是12字节，offset是8，那么12+8 > 16，就会出现错误
            //所以这里要改一下
            var total = offset + length;
            var dLength = decrypt.Length;
            if (total > dLength)
            {
                Array.Resize(ref decrypt,total);
            }
            
            Buffer.BlockCopy(decrypt, offset, result, 0, length);

            // Log.Print("解密结果："+string.Join(", ", decrypt));
            #if  UNITY_EDITOR
            EncryptedCounts++;
            #endif
            
            return result;
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
                    int tempPosition = unchecked(_origin + (int) offset);
                    if (offset < 0 || tempPosition < _origin)
                        throw new IOException("offset < 0 from the beginning of stream is invalid");
                    _position = tempPosition;
                    break;
                }
                case SeekOrigin.Current:
                {
                    int tempPosition = unchecked(_position + (int) offset);
                    if (unchecked(_position + offset) < _origin || tempPosition < _origin)
                        throw new IOException("offset is before the stream which is invalid");
                    _position = tempPosition;
                    break;
                }
                case SeekOrigin.End:
                {
                    int tempPosition = unchecked(_length + (int) offset);
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

        // Sets the length of the stream to a given value.  The new
        // value must be nonnegative and less than the space remaining in
        // the array, Int32.MaxValue - origin
        // Origin is 0 in all cases other than a JStream created on
        // top of an existing array and a specific starting offset was passed 
        // into the JStream constructor.  The upper bounds prevents any 
        // situations where a stream may be created on top of an array then 
        // the stream is made longer than the maximum possible length of the 
        // array (Int32.MaxValue).
        // 
        public override void SetLength(long value) {
            if (value < 0 || value > Int32.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(value), "value does not fit the length (out of range)");
            }
            
            // Origin wasn't publicly exposed above.
            if (value > (Int32.MaxValue - _origin)) {
                throw new ArgumentOutOfRangeException(nameof(value), "value is too big");
            }
 
            int newLength = _origin + (int)value;
            bool allocatedNewArray = EnsureCapacity(newLength);
            if (!allocatedNewArray && newLength > _length)
                Array.Clear(_buffer, _length, newLength - _length);
            _length = newLength;
            if (_position > newLength) _position = newLength;
 
        }
        
        public virtual byte[] ToArray() {
            byte[] copy = new byte[_length - _origin];
            Buffer.BlockCopy(_buffer, _origin, copy, 0, _length - _origin);
            return copy;
        }
    
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("JStream does not support write method!");
        }
    }
}
