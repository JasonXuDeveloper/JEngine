using System;

namespace Nino.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class NinoIgnoreAttribute : Attribute
	{
	}
}