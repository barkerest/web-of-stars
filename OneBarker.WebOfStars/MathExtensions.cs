namespace OneBarker.WebOfStars;

public static class MathExtensions
{
    /// <summary>
    /// Rotates a point around the origin.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static Position Rotate(this Position self, double radians)
    {
        var cr = Math.Cos(radians);
        var sr = Math.Sin(radians);
        return new Position(
            self.X * cr - self.Y * sr,
            self.X * sr + self.Y * cr
        );
    }

    private static readonly double OneDegree = Math.PI / 180.0;

    /// <summary>
    /// Converts the value provided in degrees to radians.
    /// </summary>
    /// <param name="degrees">The value in degrees.</param>
    /// <returns></returns>
    public static double DegreesToRadians(this int degrees) => degrees * OneDegree;

    /// <summary>
    /// Converts the value provided in degrees to radians.
    /// </summary>
    /// <param name="degrees">The value in degrees.</param>
    /// <returns></returns>
    public static double DegreesToRadians(this double degrees) => degrees * OneDegree;
    
}
