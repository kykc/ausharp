namespace ausharp.Extensions;

public static class Collections
{
    public static void ForEach<T>(this IEnumerable<T> subj, Action<T> action, bool parallel = false)
    {
        if (parallel)
        {
            Parallel.ForEach(subj, action);
        }
        else
        {
            foreach (var el in subj)
            {
                action(el);
            }
        }
    }
}