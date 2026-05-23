using Avalonia;
using Avalonia.Media;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;
using MaterialYou.Avalonia.DynamicColor;

namespace MaterialYou.Avalonia.DynamicColor.Tests;

public class SchemeBrushRegistryTests
{
    [Fact]
    public void Register_And_Trigger_SchemeChanged_Updates_Brush_Color()
    {
        var brush = new SolidColorBrush(Colors.Black);
        var expectedPrimary = new LightSchemeMapper().Map(CorePalette.Of(0xFF6750A4)).Primary;
        SchemeBrushRegistry.Register(brush, s => s.Primary);

        var obj = new AvaloniaObject();
        var scheme = new LightSchemeMapper().Map(CorePalette.Of(0xFF6750A4));
        MaterialColor.SetScheme(obj, scheme);

        // After SetScheme fires SchemeChanged, the brush should be updated
        var actual = (uint)(brush.Color.A << 24 | brush.Color.R << 16 | brush.Color.G << 8 | brush.Color.B);
        Assert.Equal(expectedPrimary, actual);
    }

    [Fact]
    public void Register_Multiple_Brushes_All_Updated_On_SchemeChange()
    {
        var brush1 = new SolidColorBrush(Colors.Black);
        var brush2 = new SolidColorBrush(Colors.Black);
        var scheme = new LightSchemeMapper().Map(CorePalette.Of(0xFF6750A4));

        SchemeBrushRegistry.Register(brush1, s => s.Primary);
        SchemeBrushRegistry.Register(brush2, s => s.Secondary);

        var obj = new AvaloniaObject();
        MaterialColor.SetScheme(obj, scheme);

        uint ToUint(Color c) => (uint)(c.A << 24 | c.R << 16 | c.G << 8 | c.B);
        Assert.Equal(scheme.Primary, ToUint(brush1.Color));
        Assert.Equal(scheme.Secondary, ToUint(brush2.Color));
    }

    [Fact]
    public void SchemeChanged_With_Null_Scheme_Does_Not_Throw()
    {
        var brush = new SolidColorBrush(Colors.Black);
        SchemeBrushRegistry.Register(brush, s => s.Primary);

        var obj = new AvaloniaObject();
        MaterialColor.SetScheme(obj, null);

        // Should not throw; brush color should remain unchanged
        Assert.Equal(Colors.Black, brush.Color);
    }
}
