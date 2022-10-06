using System;
using System.IO;
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
		/// Position of the current buffer
		/// </summary>
		private int _position;

		/// <summary>
		/// Position of the current buffer
		/// </summary>
		private ref int Position => ref _position;

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
		public bool EndOfReader => Position >= _length;

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
			Buffer.CopyFrom((byte*)data, 0, 0, outputLength);
			Encoding = encoding;
			Position = 0;
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
			
			return (int)DecompressAndReadNumber();
		}

		/// <summary>
		/// Decompress number for int32, int64, uint32, uint64
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		/// Read basic type from reader
		/// </summary>
		/// <param name="type"></param>
		/// <param name="result"></param>
		// ReSharper disable CognitiveComplexity
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private object AttemptReadBasicType(Type type, out bool result)
			// ReSharper restore CognitiveComplexity
		{
			result = true;
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				return wrapper.Deserialize(this);
			}

			if (TypeModel.IsEnum(type))
			{
				return DecompressAndReadEnum(type);
			}

			//比如泛型，只能list和dict
			if (type.IsGenericType)
			{
				var genericDefType = type.GetGenericTypeDefinition();
				//不是list和dict就再见了
				if (genericDefType == ConstMgr.ListDefType)
				{
					return ReadList(type);
				}

				if (genericDefType == ConstMgr.DictDefType)
				{
					return ReadDictionary(type);
				}

				result = false;
				return null;
			}

			//其他类型也不行
			if (type.IsArray)
			{
#if !ILRuntime
				if (type.GetArrayRank() > 1)
				{
					throw new NotSupportedException(
						"can not deserialize multidimensional array, use jagged array instead");
				}
#endif
				return ReadArray(type);
			}

			result = false;
			return null;
		}

		/// <summary>
		/// Read primitive value from binary writer, DO NOT USE THIS FOR CUSTOM EXPORTER
		/// </summary>
		/// <param name="type"></param>
		/// <exception cref="InvalidDataException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable CognitiveComplexity
		public object ReadCommonVal(Type type)
			// ReSharper restore CognitiveComplexity
		{
			var ret = AttemptReadBasicType(type, out bool result);
			if (result)
			{
				if (TypeModel.IsEnum(type))
				{
#if ILRuntime
					if (type is ILRuntime.Reflection.ILRuntimeType)
					{
						var baseType = Enum.GetUnderlyingType(type);
						if (baseType == typeof(long)
						    || baseType == typeof(uint)
						    || baseType == typeof(ulong))
							return Convert.ChangeType(ret, typeof(Int64));
						return Convert.ChangeType(ret, typeof(Int32));
					}
#endif
					return Enum.ToObject(type, ret);
				}
				
				return ret;
			}

			return Deserializer.Deserialize(type, ConstMgr.Null, ConstMgr.Null, Encoding, this, _option, false, true,
				false, true, true);
		}

		/// <summary>
		/// Compress and write enum
		/// </summary>
		/// <param name="enumType"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
					return DecompressAndReadNumber();
			}

			return 0;
		}

		/// <summary>
		/// Get CompressType
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref CompressType GetCompressType()
		{
			return ref *(CompressType*)(&Buffer.Data[Position++]);
		}

		/// <summary>
		/// Read a byte
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ReadByte()
		{
			if (EndOfReader) return default;

			return Buffer[Position++];
		}

		/// <summary>
		/// Read byte[]
		/// </summary>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte[] ReadBytes(int len)
		{
			if (EndOfReader) return default;

			ref var l = ref len;
			byte[] ret = new byte[l];
			fixed (byte* ptr = ret)
			{
				Buffer.CopyTo(ptr, _position, l);
				Position += l;
			}

			return ret;
		}

		/// <summary>
		/// Copy buffer to a buffer, usually buffer allocated with stackalloc
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadToBuffer(byte* ptr, int len)
		{
			ref var l = ref len;
			Buffer.CopyTo(ptr, _position, l);
			Position += l;
		}

		/// <summary>
		/// Read unmanaged type
		/// </summary>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T Read<T>(int len) where T : unmanaged
		{
			if (EndOfReader) return default;

			if (Environment.Is64BitProcess && sizeof(T) == len)
			{
				Position += len;
				return *(T*)&Buffer.Data[Position - len];
			}

			//on 32 bits has to make a copy, otherwise if cast pointer to T straight ahead, will cause crash
			T ret = default;
			byte* ptr = (byte*)&ret;
			while (len-- > 0)
			{
				*ptr++ = Buffer[Position++];
			}

			return ret;
		}

		/// <summary>
		/// Read unmanaged type
		/// </summary>
		/// <param name="val"></param>
		/// <param name="len"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable UnusedMember.Local
		public void Read<T>(ref T val, int len) where T : unmanaged
		// ReSharper restore UnusedMember.Local
		{
			if (EndOfReader)
			{
				return;
			}

			if (Environment.Is64BitProcess && sizeof(T) == len)
			{
				Position += len;
				val = *(T*)&Buffer.Data[Position - len];
				return;
			}

			//on 32 bits has to make a copy, otherwise if cast pointer to T straight ahead, will cause crash
			byte* ptr = (byte*)Unsafe.AsPointer(ref val);
			while (len-- > 0)
			{
				*ptr++ = Buffer[Position++];
			}
		}

		/// <summary>
		/// Read sbyte
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sbyte ReadSByte()
		{
			return *(sbyte*)(&Buffer.Data[Position++]);
		}

		/// <summary>
		/// Read char
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char ReadChar()
		{
			return Read<char>(ConstMgr.SizeOfUShort);
		}

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
		public short ReadInt16()
		{
			return Read<short>(ConstMgr.SizeOfShort);
		}

		/// <summary>
		/// Read ushort
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort ReadUInt16()
		{
			return Read<ushort>(ConstMgr.SizeOfUShort);
		}

		/// <summary>
		/// Read int
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadInt32()
		{
			return Read<int>(ConstMgr.SizeOfInt);
		}

		/// <summary>
		/// Read uint
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint ReadUInt32()
		{
			return Read<uint>(ConstMgr.SizeOfUInt);
		}

		/// <summary>
		/// Read long
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadInt64()
		{
			return Read<long>(ConstMgr.SizeOfLong);
		}

		/// <summary>
		/// Read ulong
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ReadUInt64()
		{
			return Read<ulong>(ConstMgr.SizeOfULong);
		}

		/// <summary>
		/// Read float
		/// </summary>
		/// <returns></returns>
		[System.Security.SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ReadSingle()
		{
			return Read<float>(ConstMgr.SizeOfUInt);
		}

		/// <summary>
		/// Read float
		/// </summary>
		/// <returns></returns>
		[System.Security.SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ReadFloat()
		{
			return ReadSingle();
		}

		/// <summary>
		/// Read double
		/// </summary>
		/// <returns></returns>
		[System.Security.SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double ReadDouble()
		{
			return Read<double>(ConstMgr.SizeOfULong);
		}

		/// <summary>
		/// Read string
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString()
		{
			if (EndOfReader) return default;

			int len = (int)DecompressAndReadNumber();
			ref var l = ref len;
			//empty string -> no gc
			if (l == 0)
			{
				return String.Empty;
			}

			//Read directly
			if (l < 1024)
			{
				byte* buf = stackalloc byte[l];
				ReadToBuffer(buf, l);
				return Encoding.GetString(buf, l);
			}

			byte* buff = (byte*)Marshal.AllocHGlobal(l);
			ReadToBuffer(buff, l);
			var ret = Encoding.GetString(buff, l);
			Marshal.FreeHGlobal((IntPtr)buff);
			return ret;
		}

		/// <summary>
		/// Read decimal
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public decimal ReadDecimal()
		{
			return Read<Decimal>(ConstMgr.SizeOfDecimal);
		}

		/// <summary>
		/// Read bool
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadBool()
		{
			if (EndOfReader) return default;

			return Buffer[Position++] == 1;
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
				var obj = ReadCommonVal(elemType);
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
				var obj = ReadCommonVal(elemType);
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
				var key = ReadCommonVal(keyType);
				//read value
				var val = ReadCommonVal(valueType);

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