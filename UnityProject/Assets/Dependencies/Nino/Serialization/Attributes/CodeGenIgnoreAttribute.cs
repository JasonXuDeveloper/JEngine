using System;

namespace Nino.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public class CodeGenIgnoreAttribute : Attribute
	{
	}
}