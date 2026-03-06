using System;
using Avalonia.Media;

namespace Pipboy.Avalonia;

/// <summary>
/// Represents a color in HSL (Hue, Saturation, Lightness) color space.
/// Used for generating monochromatic color palettes from a single primary color.
/// </summary>
public readonly struct HslColor : IEquatable<HslColor>
{
    /// <summary>Hue in degrees, range [0, 360).</summary>
    public float H { get; }

    /// <summary>Saturation, range [0, 1].</summary>
    public float S { get; }

    /// <summary>Lightness, range [0, 1].</summary>
    public float L { get; }

    public HslColor(float h, float s, float l)
    {
        H = h < 0 ? 0 : h > 360 ? 360 : h;
        S = s < 0 ? 0 : s > 1 ? 1 : s;
        L = l < 0 ? 0 : l > 1 ? 1 : l;
    }

    /// <summary>Converts an Avalonia Color to HSL.</summary>
    public static HslColor FromColor(Color color)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float max = r > g ? (r > b ? r : b) : (g > b ? g : b);
        float min = r < g ? (r < b ? r : b) : (g < b ? g : b);
        float delta = max - min;

        float l = (max + min) / 2f;
        float s = 0f;
        float h = 0f;

        if (delta > 0.00001f)
        {
            s = l < 0.5f ? delta / (max + min) : delta / (2f - max - min);

            if (Math.Abs(max - r) < 0.00001f)
                h = ((g - b) / delta) % 6f;
            else if (Math.Abs(max - g) < 0.00001f)
                h = (b - r) / delta + 2f;
            else
                h = (r - g) / delta + 4f;

            h *= 60f;
            if (h < 0f) h += 360f;
        }

        return new HslColor(h, s, l);
    }

    /// <summary>Converts this HSL color back to an Avalonia Color.</summary>
    public Color ToColor(byte alpha = 255)
    {
        if (S < 0.00001f)
        {
            byte v = (byte)(L * 255f);
            return new Color(alpha, v, v, v);
        }

        float q = L < 0.5f ? L * (1f + S) : L + S - L * S;
        float p = 2f * L - q;

        float r = HueToRgb(p, q, H / 360f + 1f / 3f);
        float g = HueToRgb(p, q, H / 360f);
        float bVal = HueToRgb(p, q, H / 360f - 1f / 3f);

        return new Color(alpha,
            (byte)(r * 255f),
            (byte)(g * 255f),
            (byte)(bVal * 255f));
    }

    /// <summary>Returns a new HslColor with the given lightness.</summary>
    public HslColor WithLightness(float lightness)
        => new HslColor(H, S, lightness < 0 ? 0 : lightness > 1 ? 1 : lightness);

    /// <summary>Returns a new HslColor with the given saturation.</summary>
    public HslColor WithSaturation(float saturation)
        => new HslColor(H, saturation < 0 ? 0 : saturation > 1 ? 1 : saturation, L);

    /// <summary>Returns a new HslColor with lightness adjusted by delta.</summary>
    public HslColor AdjustLightness(float delta)
        => WithLightness(L + delta);

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 0.5f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    public bool Equals(HslColor other)
        => Math.Abs(H - other.H) < 0.001f
        && Math.Abs(S - other.S) < 0.001f
        && Math.Abs(L - other.L) < 0.001f;

    public override bool Equals(object? obj) => obj is HslColor other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + H.GetHashCode();
            hash = hash * 31 + S.GetHashCode();
            hash = hash * 31 + L.GetHashCode();
            return hash;
        }
    }
    public static bool operator ==(HslColor left, HslColor right) => left.Equals(right);
    public static bool operator !=(HslColor left, HslColor right) => !left.Equals(right);
    public override string ToString() => $"hsl({H:F1}, {S * 100:F1}%, {L * 100:F1}%)";
}
