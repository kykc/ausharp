namespace ausharp.Flow;

public class FlowContext
{
    private readonly LinkedList<IFlowContextHandler> _handlers = [];
    private readonly HashSet<IDisposable> _disposables = [];
    
    public Func<Exception, bool> ShouldCatch => ShouldCatchImpl;
    public Func<Exception, (string, bool)> ToErrorString => ToErrorStringImpl;
    
    // For UTs
    internal IEnumerable<IFlowContextHandler> Handlers => _handlers;

    public FlowContext(IFlowContextHandler? context = null)
    {
        if (context != null) _handlers.AddFirst(context);
    }
    
    public void AddDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    public void RemoveDisposable(IDisposable disposable)
    {
        _disposables.Remove(disposable);
    }

    public IEnumerable<IDisposable> GetDisposables()
    {
        return _disposables;
    }

    public void With(IFlowContextHandler contextHandler)
    {
        _handlers.AddFirst(contextHandler);
    }

    public void Pop()
    {
        _handlers.RemoveFirst();
    }

    internal void ClearHandlers()
    {
        _handlers.Clear();
    }
    
    private (string, bool) ToErrorStringImpl(Exception ex)
    {
        foreach (var other in _handlers)
        {
            if (other.ToErrorString(ex) is (var str, true))
            {
                return (str, true);
            }
        }
        
        return (ex.Message, false);
    }

    private bool ShouldCatchImpl(Exception ex)
    {
        return _handlers.Any(x => x.ShouldCatch(ex));
    }
}

public interface IFlowContextHandler
{
    Func<Exception, bool> ShouldCatch { get; }
    Func<Exception, (string, bool)> ToErrorString { get; }
}

public class FlowContextHandler : IFlowContextHandler
{
    private readonly (Type, Delegate) _toString;
    private readonly Func<Exception, bool> _filter;

    public Func<Exception, bool> ShouldCatch => ShouldCatchImpl;
    public Func<Exception, (string, bool)> ToErrorString => ToErrorStringImpl;

    private FlowContextHandler(Func<Exception, bool> filter, (Type, Delegate) toString)
    {
        _filter = filter;
        _toString = toString;
    }

    public static FlowContextHandler Create(Func<Exception, bool> filter)
    {
        return new FlowContextHandler(filter, (typeof(Exception), (Exception e) => e.Message));
    }

    public static FlowContextHandler Create<TEx>(Func<TEx, string> toString) where TEx : Exception
    {
        return new FlowContextHandler(ex => ex is TEx, (typeof(TEx), toString));
    }

    private (string, bool) ToErrorStringImpl(Exception ex)
    {
        return _toString.Item1.IsInstanceOfType(ex) ? ((string)_toString.Item2.DynamicInvoke(ex)!, true) : (ex.Message, false);
    }

    private bool ShouldCatchImpl(Exception ex)
    {
        return _filter(ex);
    }
}