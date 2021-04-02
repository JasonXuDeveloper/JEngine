namespace ILRuntime.Runtime.Debugger
{
    public enum DebugMessageType
    {
        CSAttach,
        SCAttachResult,
        CSBindBreakpoint,
        SCBindBreakpointResult,
        SCModuleLoaded,
        SCThreadStarted,
        SCThreadEnded,
        SCBreakpointHit,
        CSDeleteBreakpoint,
        CSExecute,
        CSStep,
        SCStepComplete,
        CSResolveVariable,
        SCResolveVariableResult,
        CSResolveIndexAccess,
        SCResolveIndexAccessResult,
        CSEnumChildren,
        SCEnumChildrenResult,
    }
}
