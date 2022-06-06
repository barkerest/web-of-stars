namespace OneBarker.WebOfStars;

internal static class IdGenerator
{
    private static long _next;

    internal static long GetNext() => Interlocked.Increment(ref _next);
}
