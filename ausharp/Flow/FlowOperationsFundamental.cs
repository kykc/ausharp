using ausharp.Extensions;

namespace ausharp.Flow;

public static class FlowOperationsFundamental
{
    public static Flow<TRes> MapConcat<TSubj, TNew, TRes>(this Flow<TSubj> subj, Func<TSubj, TNew> transformer, Func<TSubj, TNew, TRes> converter)
        where TRes : class
        where TNew: class
        where TSubj: class
    {
        try
        {
            if (subj.IsVal)
            {
                var newVal = transformer(subj.UnwrapVal());

                // This value will not be seen by Flow constructor, need to check explicitly here
                if (newVal is IDisposable disposable)
                {
                    subj.Context.AddDisposable(disposable);
                }
                
                return Flow<TRes>.Val(converter(subj.UnwrapVal(), newVal), subj.Context);
            }
            else
            {
                return new(subj.UnwrapErr(), subj.Context);
            }
        }
        catch (Exception ex) when (subj.Context.ShouldCatch(ex))
        {
            return new(subj.Context.ToErrorString(ex).Item1, subj.Context);
        }
    }
    
    public static Flow<TRes> MapOr<TSubj, TRes>(this Flow<TSubj> subj, Func<TSubj, TRes?> mapper, string error)
        where TSubj : class
        where TRes : class
    {
        try
        {
            if (subj.IsVal && mapper(subj.UnwrapVal()) is { } newValue)
            {
                return Flow<TRes>.Val(newValue, subj.Context);
            }
            else
            {
                return new(subj.IsErr ? subj.UnwrapErr() : error, subj.Context);
            }
        }
        catch (Exception ex) when (subj.Context.ShouldCatch(ex))
        {
            return new(subj.Context.ToErrorString(ex).Item1, subj.Context);
        }
    }
    
    public static Flow<TRes> MapDispose<TSubj, TDisposable, TRes>(this Flow<TSubj> subj, Func<TSubj, TRes> mapper, Func<TSubj, TDisposable> disposable)
        where TRes : class
        where TSubj : class
        where TDisposable : IDisposable
    {
        try
        {
            if (subj.IsVal)
            {
                var val = mapper(subj.UnwrapVal());
                var disposableInst = disposable(subj.UnwrapVal());
                disposableInst.Dispose();
                subj.Context.RemoveDisposable(disposableInst);

                return Flow<TRes>.Val(val, subj.Context);
            }
            else
            {
                return new(subj.UnwrapErr(), subj.Context);
            }
        }
        catch (Exception ex) when (subj.Context.ShouldCatch(ex))
        {
            return new(subj.Context.ToErrorString(ex).Item1, subj.Context);
        }
    }
    
    public static Flow<TRes> BindConcat<TSubj, TNew, TRes>(this Flow<TSubj> subj, Func<TSubj, FlowContext, Flow<TNew>> binder, Func<TSubj, TNew, TRes> converter, bool changeContext = false)
        where TRes : class
        where TNew: class
        where TSubj: class
    {
        try
        {
            if (subj.IsVal)
            {
                var newVal = binder(subj.UnwrapVal(), subj.Context);

                if (newVal.IsVal)
                {
                    var ctx = changeContext ? newVal.Context : subj.Context;
                    var prevCtx = changeContext ? subj.Context : newVal.Context;
                    
                    prevCtx.GetDisposables().ForEach(x => ctx.AddDisposable(x));
                    prevCtx.GetDisposables().ToList().ForEach(x => prevCtx.RemoveDisposable(x));
                    
                    return Flow<TRes>.Val(converter(subj.UnwrapVal(), newVal.UnwrapVal()), ctx);
                }
                else
                {
                    return new(newVal.UnwrapErr(), changeContext ? newVal.Context : subj.Context);
                }
            }
            else
            {
                return new(subj.UnwrapErr(), subj.Context);
            }
        }
        catch (Exception ex) when (subj.Context.ShouldCatch(ex))
        {
            return new(subj.Context.ToErrorString(ex).Item1, subj.Context);
        }
    }
    
    

    public static Flow<TSubj> SideEffectIf<TSubj>(this Flow<TSubj> subj, Func<TSubj, FlowContext, bool> condition, Action<TSubj, FlowContext> action)
        where TSubj : class
    {
        try
        {
            if (subj.IsVal && condition(subj.UnwrapVal(), subj.Context))
            {
                action(subj.UnwrapVal(), subj.Context);
            }

            return subj;
        }
        catch (Exception ex) when (subj.Context.ShouldCatch(ex))
        {
            return new(subj.Context.ToErrorString(ex).Item1, subj.Context);
        }
    }
}