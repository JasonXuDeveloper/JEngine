using System;
using System.Text;
using Nino.Shared.IO;
using Nino.Shared.Mgr;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Local

namespace Nino.Serialization
{
	// ReSharper disable UnusedParameter.Local
	public static class Serializer
	{
		/// <summary>
		/// Default Encoding
		/// </summary>
		private static readonly Encoding DefaultEncoding = Encoding.UTF8;

		/// <summary>
		/// Custom importer delegate that writes object to writer
		/// </summary>
		internal delegate void ImporterDelegate<in T>(T val, Writer writer);

		/// <summary>
		/// Add custom importer of all type T objects
		/// </summary>
		/// <param name="action"></param>
		/// <typeparam name="T"></typeparam>
		public static void AddCustomImporter<T>(Action<T, Writer> action)
		{
			var type = typeof(T);
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				((GenericWrapper<T>)wrapper).Importer = action.Invoke;
				return;
			}

			GenericWrapper<T> genericWrapper = new GenericWrapper<T>
			{
				Importer = action.Invoke
			};
			WrapperManifest.AddWrapper(typeof(T), genericWrapper);
		}

		/// <summary>
		/// Serialize a NinoSerialize object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <param name="encoding"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] Serialize<T>(T val, Encoding encoding = null, CompressOption option = CompressOption.Zlib)
		{
			encoding = encoding ?? DefaultEncoding;
			Writer writer = ObjectPool<Writer>.Request();
			writer.Init(encoding, option);
			return Serialize(val, encoding, writer, option);
		}

		/// <summary>
		/// Serialize a NinoSerialize object
		/// </summary>
		/// <param name="val"></param>
		/// <param name="encoding"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] Serialize(object val, Encoding encoding = null, CompressOption option = CompressOption.Zlib)
		{
			encoding = encoding ?? DefaultEncoding;
			Writer writer = ObjectPool<Writer>.Request();
			writer.Init(encoding, option);
			return Serialize(val, encoding, writer, option);
		}

		/// <summary>
		/// Serialize a NinoSerialize object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <param name="encoding"></param>
		/// <param name="writer"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		private static byte[] Serialize<T>(T val, Encoding encoding, Writer writer, CompressOption option)
		{
			Type type = typeof(T);
			bool boxed = false;
			if (type == typeof(Object))
			{
				type = val.GetType();
				boxed = true;
			}

			//basic type
			if (WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				writer.Init(encoding, TypeModel.IsNonCompressibleType(type) ? CompressOption.NoCompression : option);
				if (boxed)
				{
					wrapper.Serialize(val, writer);
				}
				else
				{
					((NinoWrapperBase<T>)wrapper).Serialize(val, writer);
				}
				var ret = writer.ToBytes();
				ObjectPool<Writer>.Return(writer);
				return ret;
			}

			//code generated type
			if (TypeModel.TryGetWrapper(type, out wrapper))
			{
				//add wrapper
				WrapperManifest.AddWrapper(type, wrapper);
				//start serialize
				if (boxed)
				{
					wrapper.Serialize(val, writer);
				}
				else
				{
					((NinoWrapperBase<T>)wrapper).Serialize(val, writer);
				}
				//compress it
				var ret = writer.ToBytes();
				ObjectPool<Writer>.Return(writer);
				return ret;
			}

			//reflection type
			return Serialize(type, val, encoding ?? DefaultEncoding, writer, option, true, true, true);
		}

		/// <summary>
		/// Serialize a NinoSerialize object
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <param name="encoding"></param>
		/// <param name="writer"></param>
		/// <param name="option"></param>
		/// <param name="returnValue"></param>
		/// <param name="skipBasicCheck"></param>
		/// <param name="skipCodeGenCheck"></param>
		/// <param name="skipGenericCheck"></param>
		/// <param name="skipEnumCheck"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		// ReSharper disable CognitiveComplexity
		internal static byte[] Serialize(Type type, object value, Encoding encoding, Writer writer,
				CompressOption option = CompressOption.Zlib, bool returnValue = true, bool skipBasicCheck = false,
				bool skipCodeGenCheck = false,
				bool skipGenericCheck = false, bool skipEnumCheck = false)
			// ReSharper restore CognitiveComplexity
		{
			//ILRuntime
#if ILRuntime
			if(value is ILRuntime.Runtime.Intepreter.ILTypeInstance ins)
			{
				type = ins.Type.ReflectionType;
			}

			type = type.ResolveRealType();
#endif

			if (writer == null)
			{
				writer = ObjectPool<Writer>.Request();
				writer.Init(encoding, option);
			}

			//basic type
			if (!skipBasicCheck && WrapperManifest.TryGetWrapper(type, out var wrapper))
			{
				writer.Init(encoding, TypeModel.IsNonCompressibleType(type) ? CompressOption.NoCompression : option);
				wrapper.Serialize(value, writer);
				byte[] ret = ConstMgr.Null;
				if (returnValue)
				{
					ret = writer.ToBytes();
					ObjectPool<Writer>.Return(writer);
				}

				return ret;
			}

			//enum			
			if (!skipEnumCheck && TypeModel.IsEnum(type))
			{
				type = Enum.GetUnderlyingType(type);
				return Serialize(type, value, encoding, writer, option, returnValue);
			}

			//code generated type
			if (!skipCodeGenCheck && TypeModel.TryGetWrapper(type, out wrapper))
			{
				//add wrapper
				WrapperManifest.AddWrapper(type, wrapper);
				//start serialize
				wrapper.Serialize(value, writer);
				byte[] ret = ConstMgr.Null;
				if (returnValue)
				{
					ret = writer.ToBytes();
					ObjectPool<Writer>.Return(writer);
				}

				return ret;
			}

			//array
			if (!skipGenericCheck && type.IsArray)
			{
				writer.Write((Array)value);
				byte[] ret = ConstMgr.Null;
				if (returnValue)
				{
					ret = writer.ToBytes();
					ObjectPool<Writer>.Return(writer);
				}

				return ret;
			}

			//list, dict
			if (!skipGenericCheck && type.IsGenericType)
			{
				var genericDefType = type.GetGenericTypeDefinition();
				//不是list和dict就再见了
				if (genericDefType == ConstMgr.ListDefType)
				{
					writer.Write((IList)value);
					byte[] ret = ConstMgr.Null;
					if (returnValue)
					{
						ret = writer.ToBytes();
						ObjectPool<Writer>.Return(writer);
					}

					return ret;
				}

				if (genericDefType == ConstMgr.DictDefType)
				{
					writer.Write((IDictionary)value);
					byte[] ret = ConstMgr.Null;
					if (returnValue)
					{
						ret = writer.ToBytes();
						ObjectPool<Writer>.Return(writer);
					}

					return ret;
				}
			}

			//Get Attribute that indicates a class/struct to be serialized
			TypeModel.TryGetModel(type, out var model);

			//invalid model
			if (model != null && !model.Valid)
			{
				ObjectPool<Writer>.Return(writer);
				return ConstMgr.Null;
			}

			//generate model
			if (model == null)
			{
				model = TypeModel.CreateModel(type);
			}

			//min, max index
			ushort min = model.Min, max = model.Max;

			void Write()
			{
				//only include all model need this
				if (model.IncludeAll)
				{
					//write len
					writer.CompressAndWrite(model.Members.Count);
				}

				while (min <= max)
				{
					//prevent index not exist
					if (!model.Types.ContainsKey(min))
					{
						min++;
						continue;
					}

					//get type of that member
					type = model.Types[min];

					//only include all model need this
					if (model.IncludeAll)
					{
						var needToStore = model.Members[min];
						writer.Write(needToStore.Name);
						writer.Write(type.FullName);
					}

					//try code gen, if no code gen then reflection
					object val = GetVal(model.Members[min], value);
					//string/list/dict can be null, other cannot
					//nullable need to register custom importer
					if (val == null)
					{
						if (type == ConstMgr.StringType ||
						    (type.IsGenericType &&
						     (type.GetGenericTypeDefinition() == ConstMgr.ListDefType ||
						      type.GetGenericTypeDefinition() == ConstMgr.DictDefType)))
						{
							writer.CompressAndWrite(0);
							min++;
							continue;
						}

						throw new NullReferenceException(
							$"{type.FullName}.{model.Members[min].Name} is null, cannot serialize");
					}

					writer.WriteCommonVal(type, val);
					min++;
				}
			}

			Write();
			byte[] buf = ConstMgr.Null;
			if (returnValue)
			{
				buf = writer.ToBytes();
				ObjectPool<Writer>.Return(writer);
			}

			return buf;
		}

		/// <summary>
		/// Get value from MemberInfo
		/// </summary>
		/// <param name="info"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static object GetVal(MemberInfo info, object instance)
		{
			switch (info)
			{
				case FieldInfo fo:
					return fo.GetValue(instance);
				case PropertyInfo po:
					return po.GetValue(instance);
				default:
					return null;
			}
		}
	}
	// ReSharper restore UnusedParameter.Local
}