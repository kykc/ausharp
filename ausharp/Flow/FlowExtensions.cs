namespace ausharp.Flow;

public static class FlowExtensions
{
    public static Flow<TSubj> Flow<TSubj>(this TSubj? subj, string error, IFlowContextHandler? context = null) where TSubj : class
    {
        return Flows.ValOr(subj, error, context);
    }
        
    public static Flow<Value<TSubj>> Flow<TSubj>(this TSubj? subj, string error, IFlowContextHandler? context = null) where TSubj : struct
    {
        return subj.HasValue ? Flows.Val(new Value<TSubj>(subj.Value), context) : Flows.Err<Value<TSubj>>(error, context);
    }

    public static Flow<TSubj> Flow<TSubj>(this TSubj subj, IFlowContextHandler? context = null) where TSubj : class
    {
        return Flows.Val(subj, context);
    }

    public static Flow<None> Flow(this bool value, string error, IFlowContextHandler? context = null)
    {
        return value ? Flows.Val(None.Value, context) : Flows.Err<None>(error, context); 
    }
}