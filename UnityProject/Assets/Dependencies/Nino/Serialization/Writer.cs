using System;
using System.IO;
using System.Text;
using Nino.Shared.IO;
using Nino.Shared.Mgr;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
// ReSharper disable CognitiveComplexity

namespace Nino.Serialization
{
	/// <summary>
	/// A writer that writes serialization Data
	/// </summary>
	public class Writer
	{
		/// <summary>
		/// block size when creating buffer
		/// </summary>
		private const ushort BufferBlockSize = ushort.MaxValue;

		/// <summary>
		/// Buffer that stores data
		/// </summary>
		private ExtensibleBuffer<byte> buffer;

		/// <summary>
		/// encoding for string
		/// </summary>
		private Encoding writerEncoding;

		/// <summary>
		/// compress option
		/// </summary>
		private CompressOption option;

		/// <summary>
		/// Position of the current buffer
		/// </summary>
		private int position;

		/// <summary>
		/// Convert writer to byte
		/// </summary>
		/// <returns></returns>
		public byte[] ToBytes()
		{
			switch (option)
			{
				case CompressOption.Zlib:
					return CompressMgr.Compress(buffer, position);
				case CompressOption.Lz4:
					throw new NotSupportedException("not support lz4 yet");
				case CompressOption.NoCompression:
					return buffer.ToArray(0, position);
			}

			return ConstMgr.Null;
		}

		/// <summary>
		/// Create a writer (needs to set up values)
		/// </summary>
		public Writer()
		{

		}

		/// <summary>
		/// Create a nino writer
		/// </summary>
		/// <param name="encoding"></param>
		/// <param name="option"></param>
		public Writer(Encoding encoding, [In] CompressOption option = CompressOption.Zlib)
		{
			Init(encoding, option);
		}

		/// <summary>
		/// Init writer
		/// </summary>
		/// <param name="encoding"></param>
		/// <param name="compressOption"></param>
		public void Init(Encoding encoding, [In] CompressOption compressOption)
		{
			if (buffer == null)
			{
				var peak = ObjectPool<ExtensibleBuffer<byte>>.Peak();
				if (peak != null && peak.ExpandSize == BufferBlockSize)
				{
					buffer = ObjectPool<ExtensibleBuffer<byte>>.Request();
				}
				else
				{
					buffer = new ExtensibleBuffer<byte>(BufferBlockSize);
				}
			}

			writerEncoding = encoding;
			position = 0;
			option = compressOption;
		}

		/// <summary>
		/// Write primitive values, DO NOT USE THIS FOR CUSTOM IMPORTER
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		/// <exception cref="InvalidDataException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Obsolete("use generic method instead")]
		public void WriteCommonVal(Type type, [In] object val) =>
			Serializer.Serialize(val, writerEncoding, this, option, false);

		/// <summary>
		/// Write primitive values, DO NOT USE THIS FOR CUSTOM IMPORTER
		/// </summary>
		/// <param name="val"></param>
		/// <exception cref="InvalidDataException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteCommonVal<T>([In] T val) =>
			Serializer.Serialize(val, writerEncoding, this, option, false);

		/// <summary>
		/// Write byte[]
		/// </summary>
		/// <param name="data"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void Write([In] byte[] data)
		{
			var len = data.Length;
			CompressAndWrite(len);
			fixed (byte* ptr = data)
			{
				Write(ptr, ref len);
			}
		}

		/// <summary>
		/// Write byte[]
		/// </summary>
		/// <param name="data"></param>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe void Write([In] byte* data, ref int len)
		{
			if (len <= 8)
			{
				while (len-- > 0)
				{
					buffer[position++] = *data++;
				}

				return;
			}

			buffer.CopyFrom(data, 0, position, len);
			position += len;
		}

		/// <summary>
		/// Write unmanaged type
		/// </summary>
		/// <param name="val"></param>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Write<T>(ref T val, [In] byte len) where T : unmanaged
		{
			Unsafe.WriteUnaligned(ref buffer.AsSpan(position, len).GetPinnableReference(), val);
			position += len;
		}

		/// <summary>
		/// Write a double
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] double value)
		{
			Write(ref value, ConstMgr.SizeOfULong);
		}

		/// <summary>
		/// Write a float
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] float value)
		{
			Write(ref value, ConstMgr.SizeOfUInt);
		}

		/// <summary>
		/// Write a DateTime
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] DateTime value)
		{
			Write(value.ToOADate());
		}

		/// <summary>
		/// Write decimal
		/// </summary>
		/// <param name="d"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] decimal d)
		{
			Write(ref d, ConstMgr.SizeOfDecimal);
		}

		/// <summary>
		/// Writes a boolean to this stream. A single byte is written to the stream
		/// with the value 0 representing false or the value 1 representing true.
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] bool value)
		{
			buffer[position++] = Unsafe.As<bool, byte>(ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] char value)
		{
			Write(ref value, ConstMgr.SizeOfUShort);
		}

		/// <summary>
		/// Write string
		/// </summary>
		/// <param name="val"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void Write(string val)
		{
			if (string.IsNullOrEmpty(val))
			{
				Write((byte)CompressType.Byte);
				Write((byte)0);
				return;
			}

			int bufferSize = writerEncoding.GetMaxByteCount(val.Length);
			if (bufferSize < 1024)
			{
				byte* charBuffer = stackalloc byte[bufferSize];
				fixed (char* pValue = val)
				{
					int byteCount = writerEncoding.GetBytes(pValue, val.Length, charBuffer, bufferSize);
					CompressAndWrite(byteCount);
					Write(charBuffer, ref byteCount);
				}
			}
			else
			{
				byte* buff = (byte*)Marshal.AllocHGlobal(bufferSize);
				fixed (char* pValue = val)
				{
					// ReSharper disable AssignNullToNotNullAttribute
					int byteCount = writerEncoding.GetBytes(pValue, val.Length, buff, bufferSize);
					// ReSharper restore AssignNullToNotNullAttribute
					CompressAndWrite(byteCount);
					Write(buff, ref byteCount);
				}

				Marshal.FreeHGlobal((IntPtr)buff);
			}
		}

		#region write whole num

		/// <summary>
		/// Write byte val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] byte num)
		{
			buffer[position++] = num;
		}

		/// <summary>
		/// Write byte val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] sbyte num)
		{
			buffer[position++] = Unsafe.As<sbyte, byte>(ref num);
		}

		/// <summary>
		/// Write int val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] int num)
		{
			Write(ref num, ConstMgr.SizeOfInt);
		}

		/// <summary>
		/// Write uint val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] uint num)
		{
			Write(ref num, ConstMgr.SizeOfUInt);
		}

		/// <summary>
		/// Write short val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] short num)
		{
			Write(ref num, ConstMgr.SizeOfShort);
		}

		/// <summary>
		/// Write ushort val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] ushort num)
		{
			Write(ref num, ConstMgr.SizeOfUShort);
		}

		/// <summary>
		/// Write long val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] long num)
		{
			Write(ref num, ConstMgr.SizeOfLong);
		}

		/// <summary>
		/// Write ulong val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write([In] ulong num)
		{
			Write(ref num, ConstMgr.SizeOfULong);
		}

		#endregion

		#region write whole number without sign

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(ref ulong num)
		{
			if (num <= uint.MaxValue)
			{
				if (num <= ushort.MaxValue)
				{
					if (num <= byte.MaxValue)
					{
						buffer[position++] = (byte)CompressType.Byte;
						Write(ref num, 1);
						return;
					}

					buffer[position++] = (byte)CompressType.UInt16;
					Write(ref num, ConstMgr.SizeOfUShort);
					return;
				}

				buffer[position++] = (byte)CompressType.UInt32;
				Write(ref num, ConstMgr.SizeOfUInt);
				return;
			}

			buffer[position++] = (byte)CompressType.UInt64;
			Write(ref num, ConstMgr.SizeOfULong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(ref uint num)
		{
			if (num <= ushort.MaxValue)
			{
				if (num <= byte.MaxValue)
				{
					buffer[position++] = (byte)CompressType.Byte;
					Write(ref num, 1);
					return;
				}

				buffer[position++] = (byte)CompressType.UInt16;
				Write(ref num, ConstMgr.SizeOfUShort);
				return;
			}

			buffer[position++] = (byte)CompressType.UInt32;
			Write(ref num, ConstMgr.SizeOfUInt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite([In] ulong num)
		{
			ref var n = ref num;
			CompressAndWrite(ref n);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite([In] uint num)
		{
			ref var n = ref num;
			CompressAndWrite(ref n);
		}

		#endregion

		#region write whole number with sign

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(ref long num)
		{
			if (num < 0)
			{
				if (num >= int.MinValue)
				{
					if (num >= short.MinValue)
					{
						if (num >= sbyte.MinValue)
						{
							buffer[position++] = (byte)CompressType.SByte;
							Write(ref num, 1);
							return;
						}

						buffer[position++] = (byte)CompressType.Int16;
						Write(ref num, ConstMgr.SizeOfShort);
						return;
					}

					buffer[position++] = (byte)CompressType.Int32;
					Write(ref num, ConstMgr.SizeOfInt);
					return;
				}

				buffer[position++] = (byte)CompressType.Int64;
				Write(ref num, ConstMgr.SizeOfLong);
				return;
			}

			if (num <= int.MaxValue)
			{
				if (num <= short.MaxValue)
				{
					if (num <= byte.MaxValue)
					{
						buffer[position++] = (byte)CompressType.Byte;
						Write(ref num, 1);
						return;
					}

					buffer[position++] = (byte)CompressType.Int16;
					Write(ref num, ConstMgr.SizeOfShort);
					return;
				}

				buffer[position++] = (byte)CompressType.Int32;
				Write(ref num, ConstMgr.SizeOfInt);
				return;
			}

			buffer[position++] = (byte)CompressType.Int64;
			Write(ref num, ConstMgr.SizeOfLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(ref int num)
		{
			if (num < 0)
			{
				if (num >= short.MinValue)
				{
					if (num >= sbyte.MinValue)
					{
						buffer[position++] = (byte)CompressType.SByte;
						Write(ref num, 1);
						return;
					}

					buffer[position++] = (byte)CompressType.Int16;
					Write(ref num, ConstMgr.SizeOfShort);
					return;
				}

				buffer[position++] = (byte)CompressType.Int32;
				Write(ref num, ConstMgr.SizeOfInt);
				return;
			}

			if (num <= short.MaxValue)
			{
				if (num <= byte.MaxValue)
				{
					buffer[position++] = (byte)CompressType.Byte;
					Write(ref num, 1);
					return;
				}

				buffer[position++] = (byte)CompressType.Int16;
				Write(ref num, ConstMgr.SizeOfShort);
				return;
			}

			buffer[position++] = (byte)CompressType.Int32;
			Write(ref num, ConstMgr.SizeOfInt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite([In] long num)
		{
			ref var n = ref num;
			CompressAndWrite(ref n);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite([In] int num)
		{
			ref var n = ref num;
			CompressAndWrite(ref n);
		}

		#endregion

		/// <summary>
		/// Compress and write enum
		/// </summary>
		/// <param name="val"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void CompressAndWriteEnum<T>([In] T val)
		{
			var type = typeof(T);
			if (type == ConstMgr.ObjectType)
			{
				type = val.GetType();
				switch (TypeModel.GetTypeCode(type))
				{
					case TypeCode.Byte:
						buffer[position++] = Unsafe.Unbox<byte>(val);
						return;
					case TypeCode.SByte:
						buffer[position++] = *(byte*)Unsafe.Unbox<sbyte>(val);
						return;
					case TypeCode.Int16:
						Unsafe.As<byte, short>(ref buffer.AsSpan(position, 2).GetPinnableReference()) =
							Unsafe.Unbox<short>(val);
						position += 2;
						return;
					case TypeCode.UInt16:
						Unsafe.As<byte, ushort>(ref buffer.AsSpan(position, 2).GetPinnableReference()) =
							Unsafe.Unbox<ushort>(val);
						position += 2;
						return;
					case TypeCode.Int32:
						CompressAndWrite(ref Unsafe.Unbox<int>(val));
						return;
					case TypeCode.UInt32:
						CompressAndWrite(ref Unsafe.Unbox<uint>(val));
						return;
					case TypeCode.Int64:
						CompressAndWrite(ref Unsafe.Unbox<long>(val));
						return;
					case TypeCode.UInt64:
						CompressAndWrite(ref Unsafe.Unbox<ulong>(val));
						return;
				}

				return;
			}

			switch (TypeModel.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					Unsafe.WriteUnaligned(buffer.Data + position++, val);
					return;
				case TypeCode.Int16:
				case TypeCode.UInt16:
					Unsafe.WriteUnaligned(ref buffer.AsSpan(position, 2).GetPinnableReference(), val);
					position += 2;
					return;
				case TypeCode.Int32:
					CompressAndWrite(ref Unsafe.As<T, int>(ref val));
					return;
				case TypeCode.UInt32:
					CompressAndWrite(ref Unsafe.As<T, uint>(ref val));
					return;
				case TypeCode.Int64:
					CompressAndWrite(ref Unsafe.As<T, long>(ref val));
					return;
				case TypeCode.UInt64:
					CompressAndWrite(ref Unsafe.As<T, ulong>(ref val));
					return;
			}
		}

		/// <summary>
		/// Compress and write enum (no boxing)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Obsolete("Please re-generate nino serialize code to use the latest api")]
		public void CompressAndWriteEnum(Type type, [In] ulong val)
		{
			switch (TypeModel.GetTypeCode(type))
			{
				case TypeCode.Byte:
					Write((byte)val);
					return;
				case TypeCode.SByte:
					Write((sbyte)val);
					return;
				case TypeCode.Int16:
					Write((short)val);
					return;
				case TypeCode.UInt16:
					Write((ushort)val);
					return;
				case TypeCode.Int32:
					CompressAndWrite((int)val);
					return;
				case TypeCode.UInt32:
					CompressAndWrite((uint)val);
					return;
				case TypeCode.Int64:
					CompressAndWrite((long)val);
					return;
				case TypeCode.UInt64:
					CompressAndWrite(val);
					return;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(Array arr)
		{
			//empty
			if (arr == null || arr.Length == 0)
			{
				//write len
				CompressAndWrite(0);
				return;
			}

			//write len
			int len = arr.Length;
			CompressAndWrite(ref len);
			//write item
			int i = 0;
			while (i < len)
			{
				WriteCommonVal(arr.GetValue(i++));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(IList arr)
		{
			//empty
			if (arr == null || arr.Count == 0)
			{
				//write len
				CompressAndWrite(0);
				return;
			}

			//write len
			CompressAndWrite(arr.Count);
			//write item
			foreach (var c in arr)
			{
				WriteCommonVal(c);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(IDictionary dictionary)
		{
			//empty
			if (dictionary == null || dictionary.Count == 0)
			{
				//write len
				CompressAndWrite(0);
				return;
			}

			//write len
			int len = dictionary.Count;
			CompressAndWrite(ref len);
			//record keys
			var keys = dictionary.Keys;
			//write items
			foreach (var c in keys)
			{
				//write key
				WriteCommonVal(c);
				//write val
				WriteCommonVal(dictionary[c]);
			}
		}
	}
}