using System.Diagnostics;

namespace ausharp.Flow;

public sealed class Flow<TSubj> : IDisposable
    where TSubj : class
{
    private readonly TSubj? _value;
    private readonly string? _error;
    public bool IsErr => _error != null;
    public bool IsVal => _value != null;
    public FlowContext Context { get; }

    // This allows to construct error Flow w/o naming its type parameters explicitly
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public Flow(string error, FlowContext? context = null)
    {
#if DEBUG
        // All the internal operations on Flow should always pass the context explicitly
        // I found no better way to guard myself from making an easy mistake of omitting the context somewhere in this library
        if (context == null)
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);

            if (frame != null)
            {
                var method = frame.GetMethod();
                var callingClass = method?.DeclaringType;

                Debug.Assert(!(callingClass?.FullName?.StartsWith("ausharp.Flow") ?? false), "Flow constructed w/o context internally");
            }
        }
#endif
        _value = null;
        _error = error;
        Context = context ?? new FlowContext();
    }

    private Flow(TSubj? value, string? error, FlowContext context)
    {
        Debug.Assert(value != null || error != null, "value != null || error != null");
        
        _value = value;
        _error = error;
        Context = context;
    }

    public static Flow<TSubj> Val(TSubj value, FlowContext context)
    {
        if (value is IDisposable disposable)
        {
            context.AddDisposable(disposable);
        }
        
        return new Flow<TSubj>(value, null, context);
    }
    
    public static Flow<TSubj> Err(string error, FlowContext context)
    {
        return new Flow<TSubj>(null, error, context);
    }

    public Flow<TSubj> WithHandler(IFlowContextHandler contextHandler)
    {
        Context.With(contextHandler);
        
        return this;
    }

    public Flow<TSubj> Handle<TEx>(Func<TEx, string> handleString) where TEx : Exception
    {
        return WithHandler(FlowContextHandler.Create(handleString));
    }

    public Flow<TSubj> PopHandler()
    {
        Context.Pop();

        return this;
    }

    public Flow<TSubj> HandleAll()
    {
        return WithHandler(FlowContextHandler.Create(_ => true));
    }

    public TSubj UnwrapVal()
    {
        return _value ?? throw new NullReferenceException();
    }

    public string UnwrapErr()
    {
        return _error ?? throw new NullReferenceException();
    }

    public void Dispose()
    {
        foreach (var disposable in Context.GetDisposables().Distinct().ToList())
        {
            try
            {
                disposable.Dispose();
                Context.RemoveDisposable(disposable);
            }
            catch (Exception e)
            {
                // ignore exceptions on dispose
                Debug.WriteLine($"Exception on Flow.Dispose when disposing {disposable.GetType()}");
                Debug.WriteLine($"Exception {e.GetType()}: {e.Message}");
            }
        }
    }
}
