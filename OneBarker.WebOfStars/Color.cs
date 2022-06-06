using System.Reflection;

namespace OneBarker.WebOfStars;

public struct Color : IEquatable<Color>
{
    // make it RGBA in hex format
    private readonly uint    _value = 0x000000FF;
    private          string? _s     = null;

    private static uint R(byte b) => unchecked((uint)(b << 24));
    private static uint G(byte b) => (uint)(b << 16);
    private static uint B(byte b) => (uint)(b << 8);
    private static uint A(byte b) => b;
    private static byte R(uint v) => (byte)((v >> 24) & 0xFF);
    private static byte G(uint v) => (byte)((v >> 16) & 0xFF);
    private static byte B(uint v) => (byte)((v >> 8) & 0xFF);
    private static byte A(uint v) => (byte)(v & 0xFF);
    
    public Color(uint rgba) => _value = rgba;

    public Color(byte red, byte green, byte blue) => _value = R(red) | G(green) | B(blue) | 0xFF;

    public Color(byte red, byte green, byte blue, byte alpha) => _value = R(red) | G(green) | B(blue) | A(alpha);

    public Color(float red, float green, float blue)
        : this((byte)(Math.Clamp(red, 0, 1f) * 255), (byte)(Math.Clamp(green, 0, 1f) * 255), (byte)(Math.Clamp(blue, 0, 1f) * 255))
    {
    }

    public Color(float red, float green, float blue, float alpha)
        : this((byte)(Math.Clamp(red, 0, 1f) * 255), (byte)(Math.Clamp(green, 0, 1f) * 255), (byte)(Math.Clamp(blue, 0, 1f) * 255), (byte)(Math.Clamp(alpha, 0, 1f) * 255))
    {
    }

    public Color(string rgba)
    {
    }

    public byte RedByte => R(_value);

    public byte GreenByte => G(_value);

    public byte BlueByte => B(_value);

    public byte AlphaByte => A(_value);

    public float RedFloat => RedByte / 255f;

    public float BlueFloat => BlueByte / 255f;

    public float GreenFloat => GreenByte / 255f;

    public float AlphaFloat => AlphaByte / 255f;

    public uint ToUInt32() => _value;

    public override string ToString()
    {
        if (_s is { }) return _s;
        foreach (var known in Known)
        {
            if (known._value == _value)
            {
                return _s = known.ToString();
            }
        }

        return _s = _value.ToString("X8");
    }

    public bool Equals(Color other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return unchecked((int)_value);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }
    
    #region Known Colors

    private static Color[]? _known;

    public static Color[] Known
        => _known ??= typeof(Color)
                      .GetFields(BindingFlags.Public | BindingFlags.Static)
                      .Where(x => x.FieldType == typeof(Color))
                      .Select(x => (Color)x.GetValue(null)!)
                      .ToArray();

    public static readonly Color Black = new(0x000000FF) { _s = "Black" };

    public static readonly Color White = new(0xFFFFFFFF) { _s = "White" };

    public static readonly Color Gray = new(0xC0C0C0FF) { _s = "Gray" };

    public static readonly Color Brown = new(0x804000FF) { _s = "Brown" };

    public static readonly Color Red = new(0xFF0000FF) { _s = "Red" };

    public static readonly Color Orange = new(0xFF8000FF) { _s = "Orange" };

    public static readonly Color Yellow = new(0xFFFF00FF) { _s = "Yellow" };

    public static readonly Color Green = new(0x008000FF) { _s = "Green" };

    public static readonly Color Blue = new(0x0000FFFF) { _s = "Blue" };

    public static readonly Color Purple = new(0x8000FFFF) { _s = "Purple" };

    #endregion
}
