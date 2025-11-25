namespace ausharp.Flow;

public static class Flows
{
    public static Flow<TSubj> Val<TSubj>(TSubj value, IFlowContextHandler? context = null) where TSubj: class
    {
        return Flow<TSubj>.Val(value, new FlowContext(context));
    }

    public static Flow<TSubj> ValOr<TSubj>(TSubj? value, string error, IFlowContextHandler? context = null) where TSubj : class
    {
        return value != null ? Val(value, context) : Err<TSubj>(error, context);
    }
        
    public static Flow<TSubj> Err<TSubj>(string error, IFlowContextHandler? context = null) where TSubj: class
    {
        return Flow<TSubj>.Err(error, new FlowContext(context));
    }

    public static Flow<Value<TSubj>> RefVal<TSubj>(TSubj value, IFlowContextHandler? context = null) where TSubj : struct
    {
        return Val(value.RefVal(), context);
    }
}