using System;

namespace Nino.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public class NinoSerializeAttribute : Attribute
	{
		public readonly bool IncludeAll;
		
		/// <summary>
		/// A struct or class to be serialized and deserialized
		/// 一个结构体或一个类型的会被序列化以及反序列化
		/// <param name="includeAll">include all fields and properties</param>
		/// </summary>
		public NinoSerializeAttribute(bool includeAll = false)
		{
			IncludeAll = includeAll;
		}
	}
}