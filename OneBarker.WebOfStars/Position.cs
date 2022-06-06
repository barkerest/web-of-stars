namespace OneBarker.WebOfStars;

public struct Position
{
    public Position(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// The X coordinate.
    /// </summary>
    public readonly double X;

    /// <summary>
    /// The Y coordinate.
    /// </summary>
    public readonly double Y;
}
