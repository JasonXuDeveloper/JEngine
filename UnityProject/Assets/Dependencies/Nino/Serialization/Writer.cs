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
		private ExtensibleBuffer<byte> _buffer;

		/// <summary>
		/// Buffer that stores data
		/// </summary>
		private ref ExtensibleBuffer<byte> Buffer => ref _buffer;

		/// <summary>
		/// encoding for string
		/// </summary>
		private Encoding _encoding;

		/// <summary>
		/// encoding for string
		/// </summary>
		private ref Encoding Encoding => ref _encoding;

		/// <summary>
		/// compress option
		/// </summary>
		private CompressOption _option;
		
		/// <summary>
		/// compress option
		/// </summary>
		private ref CompressOption Option => ref _option;

		/// <summary>
		/// Position of the current buffer
		/// </summary>
		private int _position;
		
		/// <summary>
		/// Position of the current buffer
		/// </summary>
		private ref int Position => ref _position;

		/// <summary>
		/// Convert writer to byte
		/// </summary>
		/// <returns></returns>
		public byte[] ToBytes()
		{
			switch (Option)
			{
				case CompressOption.Zlib:
					return CompressMgr.Compress(Buffer, Position);
				case CompressOption.Lz4:
					throw new NotSupportedException("not support lz4 yet");
				case CompressOption.NoCompression:
					return Buffer.ToArray(0, Position);
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
		public Writer(Encoding encoding, CompressOption option = CompressOption.Zlib)
		{
			Init(encoding, option);
		}

		/// <summary>
		/// Init writer
		/// </summary>
		/// <param name="encoding"></param>
		/// <param name="compressOption"></param>
		public void Init(Encoding encoding, CompressOption compressOption)
		{
			if (Buffer == null)
			{
				var peak = ObjectPool<ExtensibleBuffer<byte>>.Peak();
				if (peak != null && peak.ExpandSize == BufferBlockSize)
				{
					Buffer = ObjectPool<ExtensibleBuffer<byte>>.Request();
				}
				else
				{
					Buffer = new ExtensibleBuffer<byte>(BufferBlockSize);
				}
			}

			Encoding = encoding;
			Position = 0;
			Option = compressOption;
		}

		/// <summary>
		/// Write basic type to writer
		/// </summary>
		/// <param name="val"></param>
		/// <param name="type"></param>
		// ReSharper disable CognitiveComplexity
		internal bool AttemptWriteBasicType<T>(Type type, T val)
			// ReSharper restore CognitiveComplexity
		{
			if (type == ConstMgr.ObjectType)
			{
				if (val == null) return false;
				//unbox
				type = val.GetType();
				//failed to unbox
				if (type == ConstMgr.ObjectType)
					return false;
			}

			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				wrapper.Serialize(val, this);
				return true;
			}

			//因为这里不是typecode，所以enum要单独检测
			if (TypeModel.IsEnum(type))
			{
				//have to box enum
				CompressAndWriteEnum(type, val);
				return true;
			}

			//basic type
			//比如泛型，只能list和dict
			if (type.IsGenericType)
			{
				var genericDefType = type.GetGenericTypeDefinition();
				//不是list和dict就再见了
				if (genericDefType == ConstMgr.ListDefType)
				{
					Write((IList)val);
					return true;
				}

				if (genericDefType == ConstMgr.DictDefType)
				{
					Write((IDictionary)val);
					return true;
				}

				return false;
			}

			//其他类型也不行
			if (type.IsArray)
			{
#if !ILRuntime
				if (type.GetArrayRank() > 1)
				{
					throw new NotSupportedException(
						"can not serialize multidimensional array, use jagged array instead");
				}
#endif
				Write(val as Array);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Write primitive values, DO NOT USE THIS FOR CUSTOM IMPORTER
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		/// <exception cref="InvalidDataException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable CognitiveComplexity
		public void WriteCommonVal<T>(Type type, T val)
			// ReSharper restore CognitiveComplexity
		{
			if (!AttemptWriteBasicType(type, val))
			{
				Serializer.Serialize(type, val, Encoding, this, Option, false, true, false, true, true);
			}
		}

		/// <summary>
		/// Write byte[]
		/// </summary>
		/// <param name="data"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void Write(byte[] data)
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
		internal unsafe void Write(byte* data, ref int len)
		{
			if (len <= 8)
			{
				while (len-- > 0)
				{
					Buffer[Position++] = *data++;
				}

				return;
			}

			Buffer.CopyFrom(data, 0, Position, len);
			Position += len;
		}

		/// <summary>
		/// Write unmanaged type
		/// </summary>
		/// <param name="val"></param>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Write<T>(ref T val, int len) where T : unmanaged
		{
			Unsafe.As<byte, T>(ref Buffer.AsSpan(_position, len).GetPinnableReference()) = val;
			Position += len;
		}

		/// <summary>
		/// Write a double
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(double value)
		{
			Write(ref value, ConstMgr.SizeOfULong);
		}

		/// <summary>
		/// Write a float
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(float value)
		{
			Write(ref value, ConstMgr.SizeOfUInt);
		}

		/// <summary>
		/// Write a DateTime
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(DateTime value)
		{
			Write(value.ToOADate());
		}

		/// <summary>
		/// Write decimal
		/// </summary>
		/// <param name="d"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(decimal d)
		{
			Write(ref d, ConstMgr.SizeOfDecimal);
		}

		/// <summary>
		/// Writes a boolean to this stream. A single byte is written to the stream
		/// with the value 0 representing false or the value 1 representing true.
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void Write(bool value)
		{
			Buffer[Position++] = *((byte*)(&value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(char value)
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

			int bufferSize = Encoding.GetMaxByteCount(val.Length);
			if (bufferSize < 1024)
			{
				byte* buffer = stackalloc byte[bufferSize];
				fixed (char* pValue = val)
				{
					int byteCount = Encoding.GetBytes(pValue, val.Length, buffer, bufferSize);
					CompressAndWrite(byteCount);
					Write(buffer, ref byteCount);
				}
			}
			else
			{
				byte* buff = (byte*)Marshal.AllocHGlobal(bufferSize);
				fixed (char* pValue = val)
				{
					// ReSharper disable AssignNullToNotNullAttribute
					int byteCount = Encoding.GetBytes(pValue, val.Length, buff, bufferSize);
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
		public void Write(byte num)
		{
			Buffer[Position++] = num;
		}

		/// <summary>
		/// Write byte val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void Write(sbyte num)
		{
			Buffer[Position++] = *(byte*)&num;
		}

		/// <summary>
		/// Write int val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(int num)
		{
			Write(ref num, ConstMgr.SizeOfInt);
		}

		/// <summary>
		/// Write uint val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(uint num)
		{
			Write(ref num, ConstMgr.SizeOfUInt);
		}

		/// <summary>
		/// Write short val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(short num)
		{
			Write(ref num, ConstMgr.SizeOfShort);
		}

		/// <summary>
		/// Write ushort val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(ushort num)
		{
			Write(ref num, ConstMgr.SizeOfUShort);
		}

		/// <summary>
		/// Write long val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(long num)
		{
			Write(ref num, ConstMgr.SizeOfLong);
		}

		/// <summary>
		/// Write ulong val to binary writer
		/// </summary>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(ulong num)
		{
			Write(ref num, ConstMgr.SizeOfULong);
		}

		#endregion

		#region write whole number without sign

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(ulong num)
		{
			ref var n = ref num;
			if (n <= uint.MaxValue)
			{
				if (n <= ushort.MaxValue)
				{
					if (n <= byte.MaxValue)
					{
						Buffer[Position++] = (byte)CompressType.Byte;
						Write(ref num, 1);
						return;
					}

					Buffer[Position++] = (byte)CompressType.UInt16;
					Write(ref num, ConstMgr.SizeOfUShort);
					return;
				}

				Buffer[Position++] = (byte)CompressType.UInt32;
				Write(ref num, ConstMgr.SizeOfUInt);
				return;
			}

			Buffer[Position++] = (byte)CompressType.UInt64;
			Write(ref num, ConstMgr.SizeOfULong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(uint num)
		{
			ref var n = ref num;
			if (n <= ushort.MaxValue)
			{
				if (n <= byte.MaxValue)
				{
					Buffer[Position++] = (byte)CompressType.Byte;
					Write(ref num, 1);
					return;
				}

				Buffer[Position++] = (byte)CompressType.UInt16;
				Write(ref num, ConstMgr.SizeOfUShort);
				return;
			}

			Buffer[Position++] = (byte)CompressType.UInt32;
			Write(ref num, ConstMgr.SizeOfUInt);
		}

		#endregion

		#region write whole number with sign

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(long num)
		{
			ref var n = ref num;
			if (n < 0)
			{
				if (n >= int.MinValue)
				{
					if (n >= short.MinValue)
					{
						if (n >= sbyte.MinValue)
						{
							Buffer[Position++] = (byte)CompressType.SByte;
							Write(ref num, 1);
							return;
						}

						Buffer[Position++] = (byte)CompressType.Int16;
						Write(ref num, ConstMgr.SizeOfShort);
						return;
					}

					Buffer[Position++] = (byte)CompressType.Int32;
					Write(ref num, ConstMgr.SizeOfInt);
					return;
				}

				Buffer[Position++] = (byte)CompressType.Int64;
				Write(ref num, ConstMgr.SizeOfLong);
				return;
			}

			if (n <= int.MaxValue)
			{
				if (n <= short.MaxValue)
				{
					if (n <= sbyte.MaxValue)
					{
						Buffer[Position++] = (byte)CompressType.SByte;
						Write(ref num, 1);
						return;
					}

					if (n <= byte.MaxValue)
					{
						Buffer[Position++] = (byte)CompressType.Byte;
						Write(ref num, 1);
						return;
					}

					Buffer[Position++] = (byte)CompressType.Int16;
					Write(ref num, ConstMgr.SizeOfShort);
					return;
				}

				Buffer[Position++] = (byte)CompressType.Int32;
				Write(ref num, ConstMgr.SizeOfInt);
				return;
			}

			Buffer[Position++] = (byte)CompressType.Int64;
			Write(ref num, ConstMgr.SizeOfLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWrite(int num)
		{
			ref var n = ref num;
			if (n < 0)
			{
				if (n >= short.MinValue)
				{
					if (n >= sbyte.MinValue)
					{
						Buffer[Position++] = (byte)CompressType.SByte;
						Write(ref num, 1);
						return;
					}

					Buffer[Position++] = (byte)CompressType.Int16;
					Write(ref num, ConstMgr.SizeOfShort);
					return;
				}

				Buffer[Position++] = (byte)CompressType.Int32;
				Write(ref num, ConstMgr.SizeOfInt);
				return;
			}

			if (n <= short.MaxValue)
			{
				if (n <= sbyte.MaxValue)
				{
					Buffer[Position++] = (byte)CompressType.SByte;
					Write(ref num, 1);
					return;
				}

				if (n <= byte.MaxValue)
				{
					Buffer[Position++] = (byte)CompressType.Byte;
					Write(ref num, 1);
					return;
				}

				Buffer[Position++] = (byte)CompressType.Int16;
				Write(ref num, ConstMgr.SizeOfShort);
				return;
			}

			Buffer[Position++] = (byte)CompressType.Int32;
			Write(ref num, ConstMgr.SizeOfInt);
		}

		#endregion

		/// <summary>
		/// Compress and write enum (no boxing)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWriteEnum(Type type, object val)
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
					CompressAndWrite((ulong)val);
					return;
			}
		}

		/// <summary>
		/// Compress and write enum (no boxing)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="val"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CompressAndWriteEnum(Type type, ulong val)
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
			CompressAndWrite(len);
			//other type
			var elemType = arr.GetValue(0)?.GetType() ?? arr.GetType().GetElementType();
			//write item
			int i = 0;
			while (i < len)
			{
				WriteCommonVal(elemType, arr.GetValue(i++));
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

			//other
			var elemType = arr[0].GetType();
			//write len
			CompressAndWrite(arr.Count);
			//write item
			foreach (var c in arr)
			{
				WriteCommonVal(elemType, c);
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
			CompressAndWrite(len);
			//record keys
			var keys = dictionary.Keys;
			Type valueType;
			var keyType = valueType = null;
			//write items
			foreach (var c in keys)
			{
				if (keyType == null)
				{
					keyType = c.GetType();
				}

				if (valueType == null)
				{
					valueType = dictionary[c].GetType();
				}

				//write key
				WriteCommonVal(keyType, c);
				//write val
				WriteCommonVal(valueType, dictionary[c]);
			}
		}
	}
}