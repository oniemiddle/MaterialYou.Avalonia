using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.VisualTree;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

internal static class SchemeBrushRegistry
{
    private static readonly ConditionalWeakTable<SolidColorBrush, Func<Scheme<uint>, uint>> s_brushes = new();
    private static bool _initialized;

    public static void Register(SolidColorBrush brush, Func<Scheme<uint>, uint> accessor)
    {
        if (!_initialized)
        {
            MaterialColor.SchemeChanged += OnSchemeChanged;
            _initialized = true;
        }
        s_brushes.AddOrUpdate(brush, accessor);
    }

    private static void OnSchemeChanged(object? sender, Scheme<uint>? scheme)
    {
        if (scheme is null) return;

        foreach (var kvp in s_brushes)
        {
            var uintColor = kvp.Value(scheme);
            kvp.Key.Color = ColorUtilities.UIntToColor(uintColor);
        }

        // In-place SolidColorBrush.Color changes don't mark individual visuals dirty.
        // Recursively invalidate every visual in the tree so updated colors are rendered.
        InvalidateAllWindows();
    }

    private static void InvalidateAllWindows()
    {
        var app = Application.Current;
        if (app?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
                InvalidateRecursive(window);
        }
    }

    private static void InvalidateRecursive(Visual visual)
    {
        visual.InvalidateVisual();
        foreach (var child in visual.GetVisualDescendants())
        {
            if (child is Visual v)
                v.InvalidateVisual();
        }
    }
}
