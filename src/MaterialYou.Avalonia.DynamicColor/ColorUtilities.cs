namespace MaterialYou.Avalonia.DynamicColor;

internal static class ColorUtilities
{
    public static Color UIntToColor(uint argb)
    {
        return Color.FromArgb(
            (byte)(argb >> 24),
            (byte)(argb >> 16),
            (byte)(argb >> 8),
            (byte)argb);
    }
}
