using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using MaterialColorUtilities.Schemes;
using MaterialYou.Avalonia.DynamicColor;

namespace MaterialYou.Avalonia.IntegrationTests.Visual;

/// <summary>
/// Verifies that MdSysColor in pseudo-class Setters survives scheme changes.
///
/// Note: Avalonia 12.0.1's headless Styler does not process pseudo-class selectors
/// targeting /template/ children. These tests verify the notification chain directly:
/// Setter context -> SchemeBrushRegistry -> in-place brush update on scheme change.
/// </summary>
public class MdSysColorPseudoClass : VisualTestBase
{
    /// <summary>
    /// ToggleSwitch unchecked Track shows SurfaceVariant (baseline).
    /// </summary>
    [Fact]
    public void ToggleSwitch_Unchecked_Track_Is_SurfaceVariant()
    {
        var toggle = new ToggleSwitch();
        var window = new Window { Content = toggle, Width = 200, Height = 200 };
        window.Show();

        var track = toggle.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(b => b.Name == "Track");
        Assert.NotNull(track);
        Assert.NotNull(track.Background);

        var actual = ((ISolidColorBrush)track.Background!).Color;
        var expected = ColorUtilities.UIntToColor(DefaultScheme.SurfaceVariant);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Directly test the notification chain: a SolidColorBrush from SchemeBrushRegistry
    /// updates its color when scheme changes. This simulates what happens when
    /// a pseudo-class Setter stores a registered brush.
    /// </summary>
    [Fact]
    public void SchemeBrushRegistry_Updates_PseudoClass_Brush()
    {
        // Simulate: MdSysColor in Setter context creates a registered brush
        var brush = new SolidColorBrush(ColorUtilities.UIntToColor(DefaultScheme.Primary));
        SchemeBrushRegistry.Register(brush, s => s.Primary);

        // Apply brush to a control via a regular Setter
        var button = new Button();
        var window = new Window { Content = button, Width = 200, Height = 200 };
        window.Show();

        // Manually apply our registered brush (simulating Setter activation)
        button.SetValue(Button.ForegroundProperty, brush, BindingPriority.StyleTrigger);

        // Verify initial color
        var actual = ((ISolidColorBrush)button.Foreground!).Color;
        var expected = ColorUtilities.UIntToColor(DefaultScheme.Primary);
        Assert.Equal(expected, actual);

        // Change scheme
        MaterialColor.SetScheme(Application.Current!, AltScheme);

        // Brush should update in-place via SchemeBrushRegistry
        var actualAlt = ((ISolidColorBrush)button.Foreground!).Color;
        var expectedAlt = ColorUtilities.UIntToColor(AltScheme.Primary);
        Assert.Equal(expectedAlt, actualAlt);
    }

    /// <summary>
    /// Same as SchemeBrushRegistry test, but at StyleTrigger priority to match
    /// pseudo-class setter behavior.
    /// </summary>
    [Fact]
    public void InPlace_BrushUpdate_At_StyleTrigger_Priority()
    {
        var brush = new SolidColorBrush(ColorUtilities.UIntToColor(DefaultScheme.Primary));
        SchemeBrushRegistry.Register(brush, s => s.Primary);

        var button = new Button();
        var window = new Window { Content = button, Width = 200, Height = 200 };
        window.Show();

        // Apply at StyleTrigger priority (same as pseudo-class),
        // using same brush instance so in-place update takes effect
        button.SetValue(Button.ForegroundProperty, brush, BindingPriority.StyleTrigger);

        // Set local value at higher priority to verify StyleTrigger is maintained
        button.Foreground = new SolidColorBrush(Colors.Black);
        // After local value removal, StyleTrigger should still be active

        // Remove local value
        button.ClearValue(Button.ForegroundProperty);

        // Should still be at StyleTrigger priority - our registered brush
        var actual = ((ISolidColorBrush)button.Foreground!).Color;
        var expected = ColorUtilities.UIntToColor(DefaultScheme.Primary);
        Assert.Equal(expected, actual);

        // Change scheme
        MaterialColor.SetScheme(Application.Current!, AltScheme);

        var actualAlt = ((ISolidColorBrush)button.Foreground!).Color;
        var expectedAlt = ColorUtilities.UIntToColor(AltScheme.Primary);
        Assert.Equal(expectedAlt, actualAlt);
    }
}
