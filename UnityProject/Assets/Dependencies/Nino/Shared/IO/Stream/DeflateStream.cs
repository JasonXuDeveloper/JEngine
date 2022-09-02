using System;
using System.IO;
using System.Threading;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Nino.Shared.IO
{
	public class DeflateStream : Stream
	{
		private delegate int ReadMethod(byte[] array, int offset, int count);

		private delegate void WriteMethod(byte[] array, int offset, int count);

		private FlexibleStream baseStream;

		private CompressionMode mode;

		private readonly bool leaveOpen;

		private bool disposed;

		private readonly DeflateStreamNative native;

		public FlexibleStream BaseStream => baseStream;

		public override bool CanRead
		{
			get
			{
				if (!disposed && mode == CompressionMode.Decompress)
				{
					return baseStream.CanRead;
				}
				return false;
			}
		}

		public override bool CanSeek => false;

		public override bool CanWrite
		{
			get
			{
				if (!disposed && mode == CompressionMode.Compress)
				{
					return baseStream.CanWrite;
				}
				return false;
			}
		}

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public DeflateStream(FlexibleStream stream, CompressionMode mode)
			: this(stream, mode, false, false)
		{
		}

		public DeflateStream(FlexibleStream stream, CompressionMode mode, bool leaveOpen)
			: this(stream, mode, leaveOpen, false)
		{
		}

		private DeflateStream(FlexibleStream compressedStream, CompressionMode mode, bool leaveOpen, bool gzip)
		{
			if (mode != CompressionMode.Compress && mode != 0)
			{
				throw new ArgumentException("mode");
			}
			baseStream = compressedStream ?? throw new ArgumentNullException(nameof(compressedStream));
			native = DeflateStreamNative.Create(compressedStream, mode, gzip);
			if (native == null)
			{
				throw new NotImplementedException("Failed to initialize zlib. You probably have an old zlib installed. Version 1.2.0.4 or later is required.");
			}
			this.mode = mode;
			this.leaveOpen = leaveOpen;
		}

		~DeflateStream()
		{
			Dispose(false);
		}

		/// <summary>
		/// Finish compressing
		/// </summary>
		public void Finish()
		{
			native.DisposeZStream();
		}

		/// <summary>
		/// Reset deflate stream
		/// </summary>
		public void Reset()
		{
			baseStream.Position = 0;
			baseStream.SetLength(0);
			native.ResetZStream(mode, false);
		}

		/// <summary>
		/// Get decompressed bytes
		/// </summary>
		/// <param name="length"></param>
		/// <param name="minLen"></param>
		/// <returns></returns>
		public unsafe IntPtr GetDecompressedBytes(out int length, int minLen)
		{
			int read;
			length = 0;
			var expandSize = minLen * 10;
			IntPtr buffer = Marshal.AllocHGlobal(expandSize);
			int maxLen = minLen * 5;
			//开始写
			while ((read = Read((byte*)buffer, length, expandSize)) != 0)
			{
				length += read;
				//如果目前读取的长度达到了buffer的一半，就需要扩容
				if (length >= maxLen)
				{
					maxLen += expandSize;
					buffer = Marshal.ReAllocHGlobal(buffer, new IntPtr(maxLen + expandSize));
				}
			}

			return buffer;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				GC.SuppressFinalize(this);
			}
			native?.Dispose(disposing);
			if (disposing && !disposed)
			{
				disposed = true;
				if (!leaveOpen)
				{
					baseStream?.Close();
					baseStream = null;
				}
			}
			base.Dispose(disposing);
		}

		private unsafe int ReadInternal(byte[] array, int offset, int count)
		{
			if (count == 0)
			{
				return 0;
			}
			fixed (byte* ptr = array)
			{
				IntPtr buffer = new IntPtr(ptr + offset);
				return native.ReadZStream(buffer, count);
			}
		}

		private unsafe int ReadInternal(byte* ptr, int offset, int count)
		{
			if (count == 0)
			{
				return 0;
			}
			IntPtr buffer = new IntPtr(ptr + offset);
			return native.ReadZStream(buffer, count);
		}
		
		public unsafe int Read(byte* array, int offset, int count)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
			if (!CanRead)
			{
				throw new InvalidOperationException("Stream does not support reading.");
			}
			if (offset < 0 || count < 0)
			{
				throw new ArgumentException("Dest or count is negative.");
			}
			return ReadInternal(array, offset, count);
		}

		public override int Read(byte[] array, int offset, int count)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
			if (!CanRead)
			{
				throw new InvalidOperationException("Stream does not support reading.");
			}
			int num = array.Length;
			if (offset < 0 || count < 0)
			{
				throw new ArgumentException("Dest or count is negative.");
			}
			if (offset > num)
			{
				throw new ArgumentException("destination offset is beyond array size");
			}
			if (offset + count > num)
			{
				throw new ArgumentException("Reading would overrun buffer");
			}
			return ReadInternal(array, offset, count);
		}

		private unsafe void WriteInternal(byte[] array, int offset, int count)
		{
			if (count != 0)
			{
				fixed (byte* ptr = array)
				{
					IntPtr buffer = new IntPtr(ptr + offset);
					native.WriteZStream(buffer, count);
				}
			}
		}
		
		public unsafe void Write(byte* array, int offset, int count)
		{
			if (count != 0)
			{
				IntPtr buffer = new IntPtr(array + offset);
				native.WriteZStream(buffer, count);
			}
		}
		
		public override void Write(byte[] array, int offset, int count)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}
			if (!CanWrite)
			{
				throw new NotSupportedException("Stream does not support writing");
			}
			if (offset > array.Length - count)
			{
				throw new ArgumentException("Buffer too small. count/offset wrong.");
			}
			WriteInternal(array, offset, count);
		}

		public override void Flush()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (CanWrite)
			{
				native.Flush();
			}
		}

		public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (!CanRead)
			{
				throw new NotSupportedException("This stream does not support reading");
			}
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Must be >= 0");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), "Must be >= 0");
			}
			if (count + offset > array.Length)
			{
				throw new ArgumentException("Buffer too small. count/offset wrong.");
			}
			return new ReadMethod(ReadInternal).BeginInvoke(array, offset, count, asyncCallback, asyncState);
		}

		public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (!CanWrite)
			{
				throw new InvalidOperationException("This stream does not support writing");
			}
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Must be >= 0");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), "Must be >= 0");
			}
			if (count + offset > array.Length)
			{
				throw new ArgumentException("Buffer too small. count/offset wrong.");
			}
			return new WriteMethod(WriteInternal).BeginInvoke(array, offset, count, asyncCallback, asyncState);
		}
		
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}
	}
	
	internal class DeflateStreamNative
	{
#if UNITY_2017_1_OR_NEWER
		private const string DllName = "MonoPosixHelper";
#else 
		private const string DllName = "Deflate";
#endif
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int UnmanagedReadOrWrite(IntPtr buffer, int length, IntPtr data);

		// ReSharper disable ClassNeverInstantiated.Local
		private sealed class SafeDeflateStreamHandle : SafeHandle
			// ReSharper restore ClassNeverInstantiated.Local
		{
			public override bool IsInvalid => handle == IntPtr.Zero;

			// ReSharper disable UnusedMember.Local
			private SafeDeflateStreamHandle()
				: base(IntPtr.Zero, true)
			{
			}

			internal SafeDeflateStreamHandle(IntPtr handle)
				: base(handle, true)
			{
			}
			// ReSharper restore UnusedMember.Local

			[MonoPInvokeCallback]
			protected override bool ReleaseHandle()
			{
				try
				{
					CloseZStream(handle);
				}
				catch
				{
					// ignored
				}

				return true;
			}
		}

		private const int BufferSize = 4096;

		private UnmanagedReadOrWrite feeder;

		private FlexibleStream baseStream;

		private SafeDeflateStreamHandle zStream;

		private GCHandle data;

		private bool disposed;
		
		private Exception lastError;

		private DeflateStreamNative()
		{
		}

		[MonoPInvokeCallback]
		public static DeflateStreamNative Create(FlexibleStream compressedStream, CompressionMode mode, bool gzip)
		{
			DeflateStreamNative deflateStreamNative = new DeflateStreamNative();
			deflateStreamNative.data = GCHandle.Alloc(deflateStreamNative);
			deflateStreamNative.feeder = ((mode == CompressionMode.Compress) ? UnmanagedWrite : new UnmanagedReadOrWrite(UnmanagedRead));
			deflateStreamNative.zStream = CreateZStream(mode, gzip, deflateStreamNative.feeder, GCHandle.ToIntPtr(deflateStreamNative.data));
			if (deflateStreamNative.zStream.IsInvalid)
			{
				deflateStreamNative.Dispose(true);
				return null;
			}
			deflateStreamNative.baseStream = compressedStream;
			return deflateStreamNative;
		}

		[MonoPInvokeCallback]
		public void DisposeZStream()
		{
			if (zStream != null && !zStream.IsInvalid)
			{
				zStream.Dispose();
			}
		}

		[MonoPInvokeCallback]
		public void ResetZStream(CompressionMode mode, bool gzip)
		{
			zStream = CreateZStream(mode, gzip, this.feeder, GCHandle.ToIntPtr(this.data));
		}

		~DeflateStreamNative()
		{
			Dispose(false);
		}

		[MonoPInvokeCallback]
		public void Dispose(bool disposing)
		{
			if (disposing && !disposed)
			{
				disposed = true;
				GC.SuppressFinalize(this);
			}
			else
			{
				baseStream = Stream.Null as FlexibleStream;
			}
			if (zStream != null && !zStream.IsInvalid)
			{
				zStream.Dispose();
			}
			_ = data;
			if (data.IsAllocated)
			{
				data.Free();
			}
		}

		[MonoPInvokeCallback]
		public void Flush()
		{
			int result = Flush(zStream);
			CheckResult(result, "Flush");
		}

		[MonoPInvokeCallback]
		public int ReadZStream(IntPtr buffer, int length)
		{
			int result = ReadZStream(zStream, buffer, length);
			CheckResult(result, "ReadInternal");
			return result;
		}

		[MonoPInvokeCallback]
		public void WriteZStream(IntPtr buffer, int length)
		{
			int result = WriteZStream(zStream, buffer, length);
			CheckResult(result, "WriteInternal");
		}

		[MonoPInvokeCallback]
		private static int UnmanagedRead(IntPtr buffer, int length, IntPtr data)
		{
			if (!(GCHandle.FromIntPtr(data).Target is DeflateStreamNative deflateStreamNative))
			{
				return -1;
			}
			return deflateStreamNative.UnmanagedRead(buffer, length);
		}

		[MonoPInvokeCallback]
		private unsafe int UnmanagedRead(IntPtr buffer, int length)
		{
			int count = Math.Min(length, BufferSize);
			int num;
			try
			{
				num = baseStream.Read((byte*)buffer, 0, count);
			}
			catch
			{
				return -12;
			}
			return num;
		}

		[MonoPInvokeCallback]
		private static int UnmanagedWrite(IntPtr buffer, int length, IntPtr data)
		{
			if (!(GCHandle.FromIntPtr(data).Target is DeflateStreamNative deflateStreamNative))
			{
				return -1;
			}
			return deflateStreamNative.UnmanagedWrite(buffer, length);
		}

		[MonoPInvokeCallback]
		private unsafe int UnmanagedWrite(IntPtr buffer, int length)
		{
			baseStream.Write((byte*)buffer, 0, length);
			return length;
		}

		private void CheckResult(int result, string where)
		{
			if (result >= 0)
			{
				return;
			}
			Exception ex = Interlocked.Exchange(ref lastError, null);
			if (ex != null)
			{
				throw ex;
			}

			string R()
			{
				switch(result)
				{
					case -1:
						return "Unknown error";
					case -2 :
						return "Internal error";
					case -3 :
						return "Corrupted data";
					case -4 :
						return "Not enough memory";
					case -5 :
						return "Internal error (no progress possible)";
					case -6 :
						return "Invalid version";
					case -10 :
						return "Invalid argument(s)";
					case -11 :
						return "IO error";
					default:
						return "Unknown error";
				}
			}
			throw new IOException( R() + " " + where);
		}

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern SafeDeflateStreamHandle CreateZStream(CompressionMode compress, bool gzip, UnmanagedReadOrWrite feeder, IntPtr data);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int CloseZStream(IntPtr stream);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int Flush(SafeDeflateStreamHandle stream);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int ReadZStream(SafeDeflateStreamHandle stream, IntPtr buffer, int length);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int WriteZStream(SafeDeflateStreamHandle stream, IntPtr buffer, int length);
	}
}
