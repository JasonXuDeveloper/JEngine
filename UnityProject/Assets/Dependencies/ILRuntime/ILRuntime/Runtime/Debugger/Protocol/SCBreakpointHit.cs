using System.Collections.Generic;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class SCBreakpointHit
    {
        public int BreakpointHashCode { get; set; }
        public int ThreadHashCode { get; set; }
        public KeyValuePair<int, StackFrameInfo[]>[] StackFrame { get; set; }
    }
}
