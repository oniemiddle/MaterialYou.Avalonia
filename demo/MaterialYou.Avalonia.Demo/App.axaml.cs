using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;
using MaterialYou.Avalonia.DynamicColor;

namespace MaterialYou.Avalonia.Demo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Set up dynamic color scheme with default seed
        var corePalette = CorePalette.Of(0xFF6750A4);
        Scheme<uint> scheme;
        if (ActualThemeVariant == ThemeVariant.Dark)
            scheme = new DarkSchemeMapper().Map(corePalette);
        else
            scheme = new LightSchemeMapper().Map(corePalette);

        MaterialColor.SetScheme(this, scheme);
        MaterialColor.SetCorePalette(this, corePalette);

        // Listen for theme changes to reapply scheme
        ActualThemeVariantChanged += (_, _) =>
        {
            var cp = MaterialColor.GetCorePalette(this);
            if (cp == null) return;
            var newScheme = ActualThemeVariant == ThemeVariant.Dark
                ? (Scheme<uint>)new DarkSchemeMapper().Map(cp)
                : new LightSchemeMapper().Map(cp);
            MaterialColor.SetScheme(this, newScheme);
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
