namespace ausharp.test;

class DisposableDummy : IDisposable
{
    public bool Disposed { get; private set; } = false;
    public Action DisposeAction { get; set; } = () => { };

    public int DummyMember { get; private set; } = 42;

    public void Dispose()
    {
        if (Disposed)
        {
            throw new InvalidOperationException();
        }
        
        DisposeAction.Invoke();
            
        Disposed = true;
    }
}