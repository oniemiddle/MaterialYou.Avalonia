namespace MaterialYou.Avalonia.DynamicColor.Tests;

public class ColorUtilitiesTests
{
    [Theory]
    [InlineData(0xFF000000, 255, 0, 0, 0)] // Black
    [InlineData(0xFFFFFFFF, 255, 255, 255, 255)] // White
    [InlineData(0xFFFF0000, 255, 255, 0, 0)] // Red
    [InlineData(0xFF00FF00, 255, 0, 255, 0)] // Green
    [InlineData(0xFF0000FF, 255, 0, 0, 255)] // Blue
    [InlineData(0x00000000, 0, 0, 0, 0)] // Transparent
    [InlineData(0x806750A4, 128, 103, 80, 164)] // Semi-transparent MD3 default
    public void UIntToColor_ConvertsCorrectly(uint argb, byte a, byte r, byte g, byte b)
    {
        var color = ColorUtilities.UIntToColor(argb);
        Assert.Equal(a, color.A);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
    }
}
