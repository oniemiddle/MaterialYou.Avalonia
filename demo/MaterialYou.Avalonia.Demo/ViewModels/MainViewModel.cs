using System;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;
using MaterialYou.Avalonia.DynamicColor;

namespace MaterialYou.Avalonia.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private uint _seedColor = 0xFF6750A4;

    [ObservableProperty]
    private bool _isDark;

    public MainViewModel()
    {
        _isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    }

    [RelayCommand]
    private void UpdateSeed(string hex)
    {
        try
        {
            SeedColor = Convert.ToUInt32(hex, 16);
        }
        catch { }
    }

    partial void OnSeedColorChanged(uint value)
    {
        UpdateScheme();
    }

    partial void OnIsDarkChanged(bool value)
    {
        UpdateScheme();
    }

    private void UpdateScheme()
    {
        var app = Application.Current;
        if (app == null) return;

        var corePalette = CorePalette.Of(SeedColor);
        Scheme<uint> scheme = IsDark
            ? new DarkSchemeMapper().Map(corePalette)
            : new LightSchemeMapper().Map(corePalette);

        MaterialColor.SetScheme(app, scheme);
        MaterialColor.SetCorePalette(app, corePalette);
    }
}
