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

    public static Flow<Value<TSubj>> RefValOr<TSubj>(TSubj? value, string error, IFlowContextHandler? context = null) where TSubj : struct
    {
        return value.HasValue ? Val(value.Value.RefVal(), context) : Err<Value<TSubj>>(error, context);
    }

    public static Flow<TSubj> FirstValOrErr<TSubj>(string? error, params Func<Flow<TSubj>>[] providers) where TSubj : class
    {
        string? lastError = null;
        
        foreach (var provider in providers)
        {
            var result = provider();
            
            if (result is { IsVal: true })
            {
                return result;
            }
            else
            {
                lastError = result.UnwrapErr();
            }
        }

        return new((error ?? lastError)!);
    }

    public static Flow<TSubj> FirstValOrLastErr<TSubj>(params Func<Flow<TSubj>>[] providers) where TSubj : class
    {
        return FirstValOrErr(null, providers);
    }
}