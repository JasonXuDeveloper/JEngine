using System;

namespace Nino.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class NinoMemberAttribute : Attribute
	{
		public readonly ushort Index;

		/// <summary>
		/// Indicate a member that can be serialized and deserialized
		/// 标明该成员可以被序列化或反序列化
		/// </summary>
		/// <param name="index">index while serializing or deserializing | 序列化或反序列化时的顺序</param>
		public NinoMemberAttribute(ushort index)
		{
			this.Index = index;
		}
	}
}