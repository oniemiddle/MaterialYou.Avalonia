using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MaterialColorUtilities.Schemes;
using MaterialYou.Avalonia.DynamicColor;

namespace MaterialYou.Avalonia.IntegrationTests.Visual;

/// <summary>
/// Verifies that MdSysColor in ControlTheme Setters correctly reflects scheme colors
/// and updates when scheme changes.
/// </summary>
public class MdSysColorSchemeChange : VisualTestBase
{
    [Fact]
    public void Button_Background_Matches_SchemePrimary()
    {
        var button = new Button();
        var window = new Window { Content = button };
        window.Show();

        var actual = ((ISolidColorBrush)button.Background!).Color;
        var expected = ColorUtilities.UIntToColor(DefaultScheme.Primary);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SchemeChange_Updates_ButtonBackground()
    {
        var button = new Button();
        var window = new Window { Content = button };
        window.Show();

        MaterialColor.SetScheme(Application.Current!, AltScheme);

        var actual = ((ISolidColorBrush)button.Background!).Color;
        var expected = ColorUtilities.UIntToColor(AltScheme.Primary);

        Assert.Equal(expected, actual);
    }
}
