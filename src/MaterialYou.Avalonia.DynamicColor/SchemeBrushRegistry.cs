using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Media;
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
    }
}
