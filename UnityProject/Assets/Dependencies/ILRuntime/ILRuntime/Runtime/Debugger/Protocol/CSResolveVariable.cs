namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class CSResolveVariable
    {
        public int ThreadHashCode { get; set; }
        public VariableReference Variable { get; set; }
    }
}
