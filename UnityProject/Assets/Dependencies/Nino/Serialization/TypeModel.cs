using System;
using System.Collections.Concurrent;
using Nino.Shared.Util;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable CognitiveComplexity

namespace Nino.Serialization
{
	/// <summary>
	/// A model of a serialized type
	/// </summary>
	internal class TypeModel
	{
		private const string HelperName = "NinoSerializationHelper";

		private const BindingFlags ReflectionFlags =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
		
		private const BindingFlags StaticReflectionFlags = ReflectionFlags | BindingFlags.Static;
		private const BindingFlags FlattenHierarchyReflectionFlags = ReflectionFlags | BindingFlags.FlattenHierarchy;
		private static Type _ninoSerializeType = typeof(NinoSerializeAttribute);
		private static Type _ninoMemberType = typeof(NinoMemberAttribute);
		private static Type _ninoIgnoreType = typeof(NinoIgnoreAttribute);

		public Dictionary<ushort, MemberInfo> Members;
		public Dictionary<ushort, Type> Types;
		public ushort Min;
		public ushort Max;
		public bool Valid;
		public bool IncludeAll;

		/// <summary>
		/// Cached Models
		/// </summary>
		private static readonly Dictionary<int, TypeModel> TypeModels = new Dictionary<int, TypeModel>(10);

		/// <summary>
		/// Cached Models
		/// </summary>
		private static readonly Dictionary<int, TypeCode> TypeCodes = new Dictionary<int, TypeCode>(30)
		{
			{ typeof(byte).GetTypeHashCode(), TypeCode.Byte },
			{ typeof(sbyte).GetTypeHashCode(), TypeCode.SByte },
			{ typeof(short).GetTypeHashCode(), TypeCode.Int16 },
			{ typeof(ushort).GetTypeHashCode(), TypeCode.UInt16 },
			{ typeof(int).GetTypeHashCode(), TypeCode.Int32 },
			{ typeof(uint).GetTypeHashCode(), TypeCode.UInt32 },
			{ typeof(long).GetTypeHashCode(), TypeCode.Int64 },
			{ typeof(ulong).GetTypeHashCode(), TypeCode.UInt64 },
			{ typeof(float).GetTypeHashCode(), TypeCode.Single },
			{ typeof(double).GetTypeHashCode(), TypeCode.Double },
			{ typeof(decimal).GetTypeHashCode(), TypeCode.Decimal },
			{ typeof(char).GetTypeHashCode(), TypeCode.Char },
			{ typeof(bool).GetTypeHashCode(), TypeCode.Boolean },
			{ typeof(string).GetTypeHashCode(), TypeCode.String },
			{ typeof(object).GetTypeHashCode(), TypeCode.Object },
			{ typeof(DateTime).GetTypeHashCode(), TypeCode.DateTime },
		};

		/// <summary>
		/// Generated helpers
		/// </summary>
		private static readonly Dictionary<int, INinoWrapper> GeneratedWrapper = new Dictionary<int, INinoWrapper>(50)
		{
			{ typeof(byte).GetTypeHashCode(), null },
			{ typeof(sbyte).GetTypeHashCode(), null },
			{ typeof(short).GetTypeHashCode(), null },
			{ typeof(ushort).GetTypeHashCode(), null },
			{ typeof(int).GetTypeHashCode(), null },
			{ typeof(uint).GetTypeHashCode(), null },
			{ typeof(long).GetTypeHashCode(), null },
			{ typeof(ulong).GetTypeHashCode(), null },
			{ typeof(bool).GetTypeHashCode(), null },
			{ typeof(float).GetTypeHashCode(), null },
			{ typeof(double).GetTypeHashCode(), null },
			{ typeof(decimal).GetTypeHashCode(), null },
			{ typeof(char).GetTypeHashCode(), null },
			{ typeof(string).GetTypeHashCode(), null },
			{ typeof(DateTime).GetTypeHashCode(), null },
		};

		/// <summary>
		/// No compression types
		/// </summary>
		internal static readonly HashSet<int> NoCompressionTypes = new HashSet<int>()
		{
			typeof(int).GetTypeHashCode(),
			typeof(uint).GetTypeHashCode(),
			typeof(long).GetTypeHashCode(),
			typeof(ulong).GetTypeHashCode(),
			typeof(byte).GetTypeHashCode(),
			typeof(sbyte).GetTypeHashCode(),
			typeof(short).GetTypeHashCode(),
			typeof(ushort).GetTypeHashCode(),
			typeof(bool).GetTypeHashCode(),
			typeof(char).GetTypeHashCode(),
			typeof(decimal).GetTypeHashCode(),
			typeof(double).GetTypeHashCode(),
			typeof(float).GetTypeHashCode(),
			typeof(DateTime).GetTypeHashCode(),
		};

		/// <summary>
		/// Cached Models
		/// </summary>
		private static readonly ConcurrentDictionary<Type, bool> IsEnumTypeCache =
			new ConcurrentDictionary<Type, bool>(3, 30);

		/// <summary>
		/// Whether or not the type is a non compress type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static bool IsNonCompressibleType(Type type)
		{
			return NoCompressionTypes.Contains(type.GetTypeHashCode()) || IsEnum(type);
		}

		/// <summary>
		/// Get a type code
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static TypeCode GetTypeCode(Type type)
		{
#if ILRuntime
			if (IsEnum(type) && type is ILRuntime.Reflection.ILRuntimeType)
			{
				type = Enum.GetUnderlyingType(type);
			}
#endif
			var hash = type.GetTypeHashCode();
			if (TypeCodes.TryGetValue(hash, out var ret))
			{
				return ret;
			}

			TypeCodes[hash] = ret = Type.GetTypeCode(type);
			return ret;
		}

		/// <summary>
		/// Get whether or not a type is enum
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static bool IsEnum(Type type)
		{
			if (IsEnumTypeCache.TryGetValue(type, out var ret)) return ret;
			IsEnumTypeCache[type] = ret = type.IsEnum;
			return ret;
		}

		/// <summary>
		/// Get whether or not a type is a code gen type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="helper"></param>
		/// <returns></returns>
		internal static bool TryGetWrapper(Type type, out INinoWrapper helper)
		{
			var hash = type.GetTypeHashCode();
			if (GeneratedWrapper.TryGetValue(hash, out helper)) return helper != null;

			var field = type.GetField(HelperName, StaticReflectionFlags);
			helper = (INinoWrapper)field?.GetValue(null);
			GeneratedWrapper[hash] = helper;
			return helper != null;
		}

		/// <summary>
		/// Try get cached model
		/// </summary>
		/// <param name="type"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		internal static void TryGetModel(Type type, out TypeModel model)
		{
			var hash = type.GetTypeHashCode();
			if (TypeModels.TryGetValue(hash, out model)) return;
			object[] ns = type.GetCustomAttributes(_ninoSerializeType, false);
			if (ns.Length != 0) return;
			model = new TypeModel()
			{
				Valid = false
			};
			TypeModels.Add(hash, model);
			throw new InvalidOperationException(
				$"The type {type.FullName} does not have NinoSerialize attribute or custom importer/exporter");
		}

		/// <summary>
		/// Thread safe lock
		/// </summary>
		private static SpinLock _createLock;

		/// <summary>
		/// Create a typeModel using given type
		/// </summary>
		/// <param name="type"></param>
		/// <exception cref="InvalidOperationException"></exception>
		// ReSharper disable CognitiveComplexity
		internal static TypeModel CreateModel(Type type)
			// ReSharper restore CognitiveComplexity
		{
			TypeModel model;
			//thread safe
			bool lockTaken = false;
			try
			{
				_createLock.Enter(ref lockTaken);
				TryGetModel(type, out model);
				if (model != null) return model;
				model = new TypeModel
				{
					Min = ushort.MaxValue,
					Max = ushort.MinValue,
					Valid = true,
					//fetch members
					Members = new Dictionary<ushort, MemberInfo>(10),
					//fetch types
					Types = new Dictionary<ushort, Type>(10)
				};

				//include all or not
				object[] ns = type.GetCustomAttributes(_ninoSerializeType, true);
				model.IncludeAll = ((NinoSerializeAttribute)ns[0]).IncludeAll;

				//store temp attr
				object[] sps;
				//flag
				ushort index;

				//fetch fields (only public and private fields that declared in the type)
				FieldInfo[] fs = type.GetFields(FlattenHierarchyReflectionFlags);
				//iterate fields
				foreach (var f in fs)
				{
					if (model.IncludeAll)
					{
						//skip nino ignore
						var ig = f.GetCustomAttributes(_ninoIgnoreType, true);
						if (ig.Length > 0) continue;
						index = (ushort)model.Members.Count;
					}
					else
					{
						sps = f.GetCustomAttributes(_ninoMemberType, true);
						//not fetch all and no attribute => skip this member
						if (sps.Length != 1) continue;
						index = ((NinoMemberAttribute)sps[0]).Index;
					}

					//record field
					model.Members.Add(index, f);
#if ILRuntime
					var t = f.FieldType;
					if (t.IsGenericType)
					{
						model.Types.Add(index, t);
					}
					else
					{
						model.Types.Add(index, t.ResolveRealType());
					}
#else
					model.Types.Add(index, f.FieldType);
#endif
					//record min/max
					if (index < model.Min)
					{
						model.Min = index;
					}

					if (index > model.Max)
					{
						model.Max = index;
					}
				}

				//fetch properties (only public and private properties that declared in the type)
				PropertyInfo[] ps = type.GetProperties(FlattenHierarchyReflectionFlags);
				//iterate properties
				foreach (var p in ps)
				{
					//has to have getter and setter
					if (!(p.CanRead && p.CanWrite))
					{
						throw new InvalidOperationException(
							$"Cannot read or write property {p.Name} in {type.FullName}, cannot Serialize or Deserialize this property");
					}

					if (model.IncludeAll)
					{
						//skip nino ignore
						var ig = p.GetCustomAttributes(_ninoIgnoreType, true);
						if (ig.Length > 0) continue;
						index = (ushort)model.Members.Count;
					}
					else
					{
						sps = p.GetCustomAttributes(_ninoMemberType, true);
						//not fetch all and no attribute => skip this member
						if (sps.Length != 1) continue;
						index = ((NinoMemberAttribute)sps[0]).Index;
					}

					//record property
					model.Members.Add(index, p);
#if ILRuntime
				var t = p.PropertyType;
				if (t.IsArray || t.IsGenericType)
				{
					model.Types.Add(index, t);
				}
				else
				{
					model.Types.Add(index, t.ResolveRealType());
				}
#else
					model.Types[index] = p.PropertyType;
#endif
					//record min/max
					if (index < model.Min)
					{
						model.Min = index;
					}

					if (index > model.Max)
					{
						model.Max = index;
					}
				}

				if (model.Members.Count == 0)
				{
					model.Valid = false;
				}

				TypeModels[type.GetTypeHashCode()] = model;
			}
			finally
			{
				if (lockTaken) _createLock.Exit(false);
			}
			return model;
		}
	}
}

