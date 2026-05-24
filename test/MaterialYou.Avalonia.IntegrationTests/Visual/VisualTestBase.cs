using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Styling;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;
using MaterialYou.Avalonia.DynamicColor;

namespace MaterialYou.Avalonia.IntegrationTests.Visual;

/// <summary>
/// Base class for headless visual tests. Initializes the headless platform once per assembly,
/// and resets scheme/theme before each test.
/// </summary>
public class VisualTestBase : IDisposable
{
    protected static readonly Scheme<uint> DefaultScheme =
        new LightSchemeMapper().Map(CorePalette.Of(0xFF6750A4));

    protected static readonly Scheme<uint> AltScheme =
        new LightSchemeMapper().Map(CorePalette.Of(0xFFFF0000));

    static VisualTestBase()
    {
        AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();

        Application.Current!.Styles.Add(new MaterialYouTheme());
    }

    protected VisualTestBase()
    {
        // Reset scheme before each test
        var corePalette = CorePalette.Of(0xFF6750A4);
        MaterialColor.SetScheme(Application.Current!, DefaultScheme);
        MaterialColor.SetCorePalette(Application.Current!, corePalette);
    }

    public void Dispose()
    {
        // No per-test cleanup needed
    }
}
