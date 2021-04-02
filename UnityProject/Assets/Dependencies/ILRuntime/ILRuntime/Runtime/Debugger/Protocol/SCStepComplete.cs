using System.Collections.Generic;

namespace ILRuntime.Runtime.Debugger.Protocol
{
    public class SCStepComplete
    {
        public int ThreadHashCode { get; set; }
        public KeyValuePair<int, StackFrameInfo[]>[] StackFrame { get; set; }
    }
}
