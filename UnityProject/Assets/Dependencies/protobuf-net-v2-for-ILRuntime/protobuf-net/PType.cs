using System;
using System.Collections.Generic;
using System.Linq;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;

namespace ProtoBuf
{
	public static class PType
	{
		private static ILRuntime.Runtime.Enviorment.AppDomain appdomain;
		private static readonly Dictionary<string, Type> ilruntimeTypes = new Dictionary<string, Type>();

		public static Type FindType(string metaName)
		{
			ilruntimeTypes.TryGetValue(metaName, out Type type);
			return type;
		}

		public static object CreateInstance(Type type)
		{
			if (!(type is ILRuntimeType)) return Activator.CreateInstance(type);
			string typeName = type.FullName;
			if (FindType(typeName) != null)
			{
				return appdomain.Instantiate(typeName);
			}

			if (typeName != null && appdomain.LoadedTypes.ContainsKey(typeName))
			{
				ilruntimeTypes[typeName] = type;
				return appdomain.Instantiate(typeName);
			}

			return null;
		}

		public static Type GetPType(object o)
		{
			Type type;
			if (o is ILTypeInstance ins)
			{
				type = ins.Type.ReflectionType;
			}
			else
			{
				type = o.GetType();
			}

			return type;
		}

		public static void RegisterILRuntimeCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain domain)
		{
			appdomain = domain;
			var allTypes = domain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
			foreach (var type in allTypes)
			{
				if (type.FullName != null) ilruntimeTypes[type.FullName] = type;
			}
		}
	}
} 