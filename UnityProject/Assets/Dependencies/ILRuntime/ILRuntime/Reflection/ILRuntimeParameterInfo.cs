using System;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;

namespace ILRuntime.Reflection
{
    public class ILRuntimeParameterInfo : ParameterInfo
    {
        IType type;
        MethodBase method;
        Mono.Cecil.ParameterDefinition definition;

        public ILRuntimeParameterInfo(Mono.Cecil.ParameterDefinition definition, IType type, MethodBase method)
        {
            this.type = type;
            this.method = method;
            this.MemberImpl = method;
            this.definition = definition;
            NameImpl = definition.Name;
        }
        public override Type ParameterType
        {
            get
            {
                return type.ReflectionType;
            }
        }
    }
}
