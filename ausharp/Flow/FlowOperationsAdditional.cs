namespace ausharp.Flow;

public static class FlowOperationsAdditional
{
    public static Flow<TRes> MapDispose<TSubj, TRes>(this Flow<TSubj> subj, Func<TSubj, TRes> mapper)
        where TRes : class
        where TSubj : class, IDisposable
    {
        return subj.MapDispose(mapper, _ => subj.UnwrapVal());
    }
    
    public static Flow<TRes> BindConcat<TSubj, TNew, TRes>(this Flow<TSubj> subj, Func<TSubj, Flow<TNew>> binder, Func<TSubj, TNew, TRes> converter, bool changeContext = false)
        where TRes : class
        where TNew: class
        where TSubj: class
    {
        Flow<TNew> BinderWrapper(TSubj s, FlowContext ctx) => binder(s);

        return subj.BindConcat(BinderWrapper, converter, changeContext);
    }

    public static Flow<TSubj> SideEffect<TSubj>(this Flow<TSubj> subj, Action<TSubj> action) where TSubj : class
    {
        return subj.SideEffectIf((_, _) => true, (val, _) => action(val));
    }

    public static Flow<TSubj> SideEffect<TSubj>(this Flow<TSubj> subj, Action<TSubj, FlowContext> action) where TSubj : class
    {
        return subj.SideEffectIf((_, _) => true, action);
    }

    internal static Flow<TSubj> GetContext<TSubj>(this Flow<TSubj> subj, Action<FlowContext> action) where TSubj : class
    {
        action(subj.Context);

        return subj;
    }
    
    public static Flow<TRes> Bind<TSubj, TRes>(this Flow<TSubj> subj, Func<TSubj, Flow<TRes>> binder, bool switchContext = false)
        where TSubj : class
        where TRes : class
    {
        return subj.BindConcat(binder, (_, right) => right, switchContext);
    }
    
    public static Flow<TRes> Bind<TSubj, TRes>(this Flow<TSubj> subj, Func<TSubj, FlowContext, Flow<TRes>> binder, bool switchContext = false)
        where TSubj : class
        where TRes : class
    {
        return subj.BindConcat(binder, (_, right) => right, switchContext);
    }

    public static Flow<TSubj> Err<TSubj>(this Flow<TSubj> subj, string error) where TSubj : class
    {
        return new(error, subj.Context);
    }

    public static Flow<TSubj> Check<TSubj>(this Flow<TSubj> subj, Func<TSubj, bool> predicate, Func<TSubj, string> error) where TSubj : class
    {
        return subj.CheckIf(_ => true, predicate, error);
    }

    public static Flow<TSubj> CheckIf<TSubj>(this Flow<TSubj> subj, Func<TSubj, bool> condition, Func<TSubj, bool> predicate, Func<TSubj, string> error)
        where TSubj : class
    {
        Flow<TSubj> Binder(TSubj s) => predicate(s) ? subj : new(error(s), subj.Context);

        return subj.BindErrIf(condition, Binder);
    }

    public static Flow<TRes> Map<TSubj, TRes>(this Flow<TSubj> subj, Func<TSubj, TRes> mapper)
        where TSubj : class
        where TRes : class
    {
        return subj.MapConcat(mapper, (_, newVal) => newVal);
    }
    
    public static Flow<TSubj> BindErrIf<TSubj, TOther>(this Flow<TSubj> subj, Func<TSubj, bool> condition, Func<TSubj, Flow<TOther>> binder, bool switchContext = false) 
        where TSubj : class
        where TOther : class
    {
        Flow<TOther> BinderAdapter(TSubj s, FlowContext ctx) => binder(s);

        return subj.BindErrIf(condition, BinderAdapter, switchContext);
    }
    
    public static Flow<TSubj> BindErrIf<TSubj, TOther>(this Flow<TSubj> subj, Func<TSubj, bool> condition, Func<TSubj, FlowContext, Flow<TOther>> binder, bool switchContext = false) 
        where TSubj : class
        where TOther : class
    {
        try
        {
            if (!subj.IsVal || !condition(subj.UnwrapVal())) return subj;

            return subj.BindConcat(binder, (left, _) => left, switchContext);
        }
        catch (Exception ex) when (subj.Context.ShouldCatch(ex))
        {
            return new(subj.Context.ToErrorString(ex).Item1, subj.Context);
        }
    }
    
    public static Flow<TSubj> BindErr<TSubj, TOther>(this Flow<TSubj> subj, Func<TSubj, Flow<TOther>> binder, bool switchContext = false) 
        where TSubj : class
        where TOther : class
    {
        return subj.BindConcat(binder, (left, _) => left, switchContext);
    }
    
    public static Flow<TSubj> BindErr<TSubj, TOther>(this Flow<TSubj> subj, Func<TSubj, FlowContext, Flow<TOther>> binder, bool switchContext = false) 
        where TSubj : class
        where TOther : class
    {
        return subj.BindConcat(binder, (left, _) => left, switchContext);
    }
    
    public static bool LogErrorIfAny<TSubj>(this Flow<TSubj> subj, Action<string> logError) where TSubj : class
    {
        if (!subj.IsErr) return false;
        
        logError(subj.UnwrapErr());

        return true;
    }
}