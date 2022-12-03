using System;
using System.Text;
using Nino.Shared.IO;
using Nino.Shared.Mgr;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nino.Serialization
{
	/// <summary>
	/// A read that Reads serialization Data
	/// </summary>
	// ReSharper disable CognitiveComplexity
	public unsafe class Reader
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
		private Encoding readerEncoding;

		/// <summary>
		/// Position of the current buffer
		/// </summary>
		private int position;

		/// <summary>
		/// Length of the current reader
		/// </summary>
		private int _length;

		/// <summary>
		/// compress option
		/// </summary>
		private CompressOption _option;

		/// <summary>
		/// End of Reader
		/// </summary>
		public bool EndOfReader => position >= _length;

		/// <summary>
		/// Create an empty reader, need to set up values
		/// </summary>
		public Reader()
		{

		}

		/// <summary>
		/// Create a nino read
		/// </summary>
		/// <param name="data"></param>
		/// <param name="outputLength"></param>
		/// <param name="encoding"></param>
		/// <param name="option"></param>
		public Reader(byte[] data, int outputLength, Encoding encoding, CompressOption option = CompressOption.Zlib)
		{
			Init(data, outputLength, encoding, option);
		}

		/// <summary>
		/// Create a nino read
		/// </summary>
		/// <param name="data"></param>
		/// <param name="outputLength"></param>
		/// <param name="encoding"></param>
		/// <param name="option"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Init(IntPtr data, ref int outputLength, Encoding encoding, CompressOption option)
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

			buffer.CopyFrom((byte*)data, 0, 0, outputLength);
			readerEncoding = encoding;
			position = 0;
			_length = outputLength;
			_option = option;
			if (_option == CompressOption.Zlib)
			{
				Marshal.FreeHGlobal(data);
			}
		}

		/// <summary>
		/// Create a nino reader
		/// </summary>
		/// <param name="data"></param>
		/// <param name="outputLength"></param>
		/// <param name="encoding"></param>
		/// <param name="compressOption"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(Span<byte> data, int outputLength, Encoding encoding, CompressOption compressOption)
		{
			switch (compressOption)
			{
				case CompressOption.NoCompression:
					fixed (byte* ptr = &data.GetPinnableReference())
					{
						Init((IntPtr)ptr, ref outputLength, encoding, compressOption);
					}

					break;
				case CompressOption.Lz4:
					throw new NotSupportedException("not support lz4 yet");
				case CompressOption.Zlib:
					Init(CompressMgr.Decompress(data.ToArray(), out var length), ref length, encoding, compressOption);
					break;
			}
		}

		/// <summary>
		/// Create a nino reader
		/// </summary>
		/// <param name="data"></param>
		/// <param name="outputLength"></param>
		/// <param name="encoding"></param>
		/// <param name="compressOption"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(byte[] data, int outputLength, Encoding encoding, CompressOption compressOption)
		{
			switch (compressOption)
			{
				case CompressOption.NoCompression:
					fixed (byte* ptr = data)
					{
						Init((IntPtr)ptr, ref outputLength, encoding, compressOption);
					}

					break;
				case CompressOption.Lz4:
					throw new NotSupportedException("not support lz4 yet");
				case CompressOption.Zlib:
					Init(CompressMgr.Decompress(data, out var length), ref length, encoding, compressOption);
					break;
			}
		}

		/// <summary>
		/// Get Length
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadLength()
		{
			if (EndOfReader) return default;

			return DecompressAndReadNumber<int>();
		}

		/// <summary>
		/// Read primitive value from binary writer, DO NOT USE THIS FOR CUSTOM EXPORTER
		/// Compress and write enum
		/// </summary>
		/// <param name="type"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Obsolete("use generic method instead")]
		// ReSharper disable CognitiveComplexity
		public object ReadCommonVal(Type type) =>
			Deserializer.Deserialize(type, ConstMgr.Null, ConstMgr.Null, readerEncoding, this, _option, false);

		/// <summary>
		/// Read primitive value from binary writer, DO NOT USE THIS FOR CUSTOM EXPORTER
		/// Compress and write enum
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable CognitiveComplexity
		public T ReadCommonVal<T>() =>
			Deserializer.Deserialize<T>(buffer.AsSpan(position, _length - position), readerEncoding, this, _option,
				false);

		/// <summary>
		/// Decompress number for int32, int64, uint32, uint64
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Obsolete("use generic method instead")]
		public ulong DecompressAndReadNumber()
		{
			if (EndOfReader) return default;

			ref var type = ref GetCompressType();
			switch (type)
			{
				case CompressType.Byte:
					return ReadByte();
				case CompressType.SByte:
					return (ulong)ReadSByte();
				case CompressType.Int16:
					return (ulong)ReadInt16();
				case CompressType.UInt16:
					return ReadUInt16();
				case CompressType.Int32:
					return (ulong)ReadInt32();
				case CompressType.UInt32:
					return ReadUInt32();
				case CompressType.Int64:
					return (ulong)ReadInt64();
				case CompressType.UInt64:
					return ReadUInt64();
				default:
					throw new InvalidOperationException("invalid compress type");
			}
		}

		/// <summary>
		/// Decompress number for int32, int64, uint32, uint64
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T DecompressAndReadNumber<T>() where T : unmanaged
		{
			T result = default;
			DecompressAndReadNumber(ref result);
			return result;
		}

		/// <summary>
		/// Decompress number for int32, int64, uint32, uint64
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DecompressAndReadNumber<T>(ref T result) where T : unmanaged
		{
			if (EndOfReader)
			{
				result = default;
				return;
			}

			ref var type = ref GetCompressType();
			switch (type)
			{
				case CompressType.Byte:
					Unsafe.As<T, byte>(ref result) = ReadByte();
					return;
				case CompressType.SByte:
					switch (result)
					{
						case int _:
							var temp = (int)ReadSByte();
							Unsafe.As<T, int>(ref result) = temp;
							return;
						case long _:
							var temp2 = (long)ReadSByte();
							Unsafe.As<T, long>(ref result) = temp2;
							return;
						default:
							throw new InvalidOperationException("invalid compress type");
					}
				case CompressType.Int16:
					Unsafe.As<T, short>(ref result) = ReadInt16();
					return;
				case CompressType.UInt16:
					Unsafe.As<T, ushort>(ref result) = ReadUInt16();
					return;
				case CompressType.Int32:
					Unsafe.As<T, int>(ref result) = ReadInt32();
					return;
				case CompressType.UInt32:
					Unsafe.As<T, uint>(ref result) = ReadUInt32();
					return;
				case CompressType.Int64:
					Unsafe.As<T, long>(ref result) = ReadInt64();
					return;
				case CompressType.UInt64:
					Unsafe.As<T, ulong>(ref result) = ReadUInt64();
					return;
				default:
					throw new InvalidOperationException("invalid compress type");
			}
		}

		/// <summary>
		/// Compress and write enum
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T DecompressAndReadEnum<T>()
		{
			T val = default;
			DecompressAndReadEnum(ref val);
			return val;
		}

		/// <summary>
		/// Compress and write enum
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Obsolete("use generic method instead")]
		public ulong DecompressAndReadEnum(Type enumType)
		{
			if (EndOfReader) return default;

			switch (TypeModel.GetTypeCode(enumType))
			{
				case TypeCode.Byte:
					return ReadByte();
				case TypeCode.SByte:
					return (ulong)ReadSByte();
				case TypeCode.Int16:
					return (ulong)ReadInt16();
				case TypeCode.UInt16:
					return ReadUInt16();
				//need to consider compress
				case TypeCode.Int32:
					return (ulong)DecompressAndReadNumber<int>();
				case TypeCode.UInt32:
					return DecompressAndReadNumber<uint>();
				case TypeCode.Int64:
					return (ulong)DecompressAndReadNumber<long>();
				case TypeCode.UInt64:
					return DecompressAndReadNumber<ulong>();
			}

			return 0;
		}

		/// <summary>
		/// Compress and write enum
		/// </summary>
		/// <param name="val"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DecompressAndReadEnum<T>(ref T val)
		{
			if (EndOfReader) return;

			switch (TypeModel.GetTypeCode(typeof(T)))
			{
				case TypeCode.Byte:
					Unsafe.As<T, byte>(ref val) = ReadByte();
					return;
				case TypeCode.SByte:
					Unsafe.As<T, sbyte>(ref val) = ReadSByte();
					return;
				case TypeCode.Int16:
					Unsafe.As<T, short>(ref val) = ReadInt16();
					return;
				case TypeCode.UInt16:
					Unsafe.As<T, ushort>(ref val) = ReadUInt16();
					return;
				//need to consider compress
				case TypeCode.Int32:
					Unsafe.As<T, int>(ref val) = DecompressAndReadNumber<int>();
					return;
				case TypeCode.UInt32:
					Unsafe.As<T, uint>(ref val) = DecompressAndReadNumber<uint>();
					return;
				case TypeCode.Int64:
					Unsafe.As<T, long>(ref val) = DecompressAndReadNumber<long>();
					return;
				case TypeCode.UInt64:
					Unsafe.As<T, ulong>(ref val) = DecompressAndReadNumber<ulong>();
					return;
			}
		}

		/// <summary>
		/// Get CompressType
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref CompressType GetCompressType() => ref *(CompressType*)(&buffer.Data[position++]);

		/// <summary>
		/// Read a byte
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ReadByte()
		{
			if (EndOfReader) return default;

			return buffer[position++];
		}

		/// <summary>
		/// Read byte[]
		/// </summary>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ReadBytes([In] int len)
		{
			if (EndOfReader) return default;

			byte[] ret = new byte[len];
			fixed (byte* ptr = ret)
			{
				buffer.CopyTo(ptr, position, len);
				position += len;
			}

			return ret;
		}

		/// <summary>
		/// Copy buffer to a buffer, usually buffer allocated with stackalloc
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadToBuffer([In] byte* ptr, [In] int len)
		{
			buffer.CopyTo(ptr, position, len);
			position += len;
		}

		/// <summary>
		/// Read unmanaged type
		/// </summary>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T Read<T>([In] int len) where T : unmanaged
		{
			if (EndOfReader)
			{
				return default;
			}

			ref var pin = ref *(buffer.Data + position);
			position += len;
			return Unsafe.ReadUnaligned<T>(ref pin);
		}

		/// <summary>
		/// Read unmanaged type
		/// </summary>
		/// <param name="val"></param>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable UnusedMember.Local
		public void Read<T>(ref T val, [In] int len) where T : unmanaged
		// ReSharper restore UnusedMember.Local
		{
			if (EndOfReader)
			{
				return;
			}

			val = Unsafe.ReadUnaligned<T>(ref *(buffer.Data + position));
			position += len;
		}

		/// <summary>
		/// Read sbyte
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sbyte ReadSByte() => Unsafe.As<byte, sbyte>(ref buffer.Data[position++]);

		/// <summary>
		/// Read char
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char ReadChar() => Read<char>(ConstMgr.SizeOfUShort);

		/// <summary>
		/// Read DateTime
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime ReadDateTime()
		{
			if (EndOfReader) return default;

			return DateTime.FromOADate(ReadDouble());
		}

		/// <summary>
		/// Read short
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short ReadInt16() => Read<short>(ConstMgr.SizeOfShort);

		/// <summary>
		/// Read ushort
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort ReadUInt16() => Read<ushort>(ConstMgr.SizeOfUShort);

		/// <summary>
		/// Read int
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadInt32() => Read<int>(ConstMgr.SizeOfInt);

		/// <summary>
		/// Read uint
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint ReadUInt32() => Read<uint>(ConstMgr.SizeOfUInt);

		/// <summary>
		/// Read long
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadInt64() => Read<long>(ConstMgr.SizeOfLong);

		/// <summary>
		/// Read ulong
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ReadUInt64() => Read<ulong>(ConstMgr.SizeOfULong);

		/// <summary>
		/// Read float
		/// </summary>
		/// <returns></returns>
		[System.Security.SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ReadSingle() => Read<float>(ConstMgr.SizeOfUInt);

		/// <summary>
		/// Read float
		/// </summary>
		/// <returns></returns>
		[System.Security.SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ReadFloat() => ReadSingle();

		/// <summary>
		/// Read double
		/// </summary>
		/// <returns></returns>
		[System.Security.SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double ReadDouble() => Read<double>(ConstMgr.SizeOfULong);

		/// <summary>
		/// Read decimal
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public decimal ReadDecimal() => Read<Decimal>(ConstMgr.SizeOfDecimal);

		/// <summary>
		/// Read bool
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadBool()
		{
			if (EndOfReader) return default;

			return buffer[position++] == 1;
		}

		/// <summary>
		/// Read string
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString()
		{
			if (EndOfReader) return default;

			int len = DecompressAndReadNumber<int>();
			//empty string -> no gc
			if (len == 0)
			{
				return String.Empty;
			}

			//Read directly
			if (len < 1024)
			{
				byte* buf = stackalloc byte[len];
				ReadToBuffer(buf, len);
				return readerEncoding.GetString(buf, len);
			}

			byte* buff = (byte*)Marshal.AllocHGlobal(len);
			ReadToBuffer(buff, len);
			var ret = readerEncoding.GetString(buff, len);
			Marshal.FreeHGlobal((IntPtr)buff);
			return ret;
		}

		/// <summary>
		/// Read Array
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Array ReadArray(Type type)
		{
			if (EndOfReader) return default;

			//basic type
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				var ret = wrapper.Deserialize(this);
				return (Array)ret;
			}

			//other type
			var elemType = type.GetElementType();
			if (elemType == null)
			{
				throw new NullReferenceException("element type is null, can not make array");
			}

			//read len
			int len = ReadLength();

			Array arr = Array.CreateInstance(elemType, len);
			//read item
			int i = 0;
			while (i < len)
			{
				var obj = Deserializer.Deserialize(elemType, ConstMgr.Null, ConstMgr.Null, readerEncoding, this,
					_option,
					false);
#if ILRuntime
				arr.SetValue(ILRuntime.CLR.Utils.Extensions.CheckCLRTypes(elemType, obj), i++);
				continue;
#else
				arr.SetValue(obj, i++);
#endif
			}

			return arr;
		}

		/// <summary>
		/// Read list
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IList ReadList(Type type)
		{
			if (EndOfReader) return default;

			//basic type
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				var ret = wrapper.Deserialize(this);
				return (IList)ret;
			}

			//other
			var elemType = type.GenericTypeArguments[0];
#if ILRuntime
			if (type is ILRuntime.Reflection.ILRuntimeWrapperType wt)
			{
				elemType = wt?.CLRType.GenericArguments[0].Value.ReflectionType;
			}

			if(!elemType.IsGenericType)
			{
				elemType = elemType.ResolveRealType();
			}
#endif

			//read len
			int len = ReadLength();

			IList arr = Activator.CreateInstance(type, ConstMgr.EmptyParam) as IList;
			//read item
			int i = 0;
			while (i++ < len)
			{
				var obj = Deserializer.Deserialize(elemType, ConstMgr.Null, ConstMgr.Null, readerEncoding, this,
					_option,
					false);
#if ILRuntime
				arr?.Add(ILRuntime.CLR.Utils.Extensions.CheckCLRTypes(elemType, obj));
				continue;
#else
				arr?.Add(obj);
#endif
			}

			return arr;
		}

		/// <summary>
		/// Read Dictionary
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDictionary ReadDictionary(Type type)
		{
			if (EndOfReader) return default;

			//parse dict type
			var args = type.GetGenericArguments();
			Type keyType = args[0];
#if ILRuntime
			if (type is ILRuntime.Reflection.ILRuntimeWrapperType wt)
			{
				keyType = wt?.CLRType.GenericArguments[0].Value.ReflectionType;
			}

			if(!keyType.IsGenericType)
			{
				keyType = keyType.ResolveRealType();
			}
#endif
			Type valueType = args[1];
#if ILRuntime
			if (type is ILRuntime.Reflection.ILRuntimeWrapperType wt2)
			{
				valueType = wt2?.CLRType.GenericArguments[1].Value.ReflectionType;
			}
			
			if(!valueType.IsGenericType)
			{
				valueType = valueType.ResolveRealType();
			}
#endif

			var dict = Activator.CreateInstance(type) as IDictionary;

			//read len
			int len = ReadLength();

			//read item
			int i = 0;
			while (i++ < len)
			{
				//read key
				var key = Deserializer.Deserialize(keyType, ConstMgr.Null, ConstMgr.Null, readerEncoding, this, _option,
					false);
				//read value
				var val = Deserializer.Deserialize(valueType, ConstMgr.Null, ConstMgr.Null, readerEncoding, this,
					_option,
					false);

				//add
#if ILRuntime
				dict?.Add(ILRuntime.CLR.Utils.Extensions.CheckCLRTypes(keyType, key),
							ILRuntime.CLR.Utils.Extensions.CheckCLRTypes(valueType, val));
				continue;
#else
				dict?.Add(key, val);
#endif
			}

			return dict;
		}
	}
	// ReSharper restore CognitiveComplexity
}