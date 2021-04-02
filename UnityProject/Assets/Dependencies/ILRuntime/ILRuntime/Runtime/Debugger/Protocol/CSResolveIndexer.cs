namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class CSResolveIndexer
    {
        public int ThreadHashCode { get; set; }
        public VariableReference Index { get; set; }
        public VariableReference Body { get; set; }
    }
}
