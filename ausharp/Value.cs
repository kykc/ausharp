namespace ausharp;

public class Value<TSubj>(TSubj value)
    where TSubj : struct
{
    public TSubj Val { get; private set; } = value;
}

public class None
{
    private None()
    {
    }

    public static readonly None Value = new();
}

public static class ValueExtensions
{
    public static Value<T> RefVal<T>(this T value) where T: struct => new(value);
}