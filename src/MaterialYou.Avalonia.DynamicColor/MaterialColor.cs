using System;
using Avalonia;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MaterialColor
{
    public static readonly AttachedProperty<Scheme<uint>?> SchemeProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, Scheme<uint>?>("Scheme");

    public static readonly AttachedProperty<CorePalette?> CorePaletteProperty =
        AvaloniaProperty.RegisterAttached<MaterialColor, AvaloniaObject, CorePalette?>("CorePalette");

    public static void SetScheme(AvaloniaObject obj, Scheme<uint>? value)
    {
        obj.SetValue(SchemeProperty, value);
        SchemeChanged?.Invoke(null, value);
    }

    public static Scheme<uint>? GetScheme(AvaloniaObject obj) => obj.GetValue(SchemeProperty);

    public static void SetCorePalette(AvaloniaObject obj, CorePalette? value)
    {
        obj.SetValue(CorePaletteProperty, value);
    }

    public static CorePalette? GetCorePalette(AvaloniaObject obj) => obj.GetValue(CorePaletteProperty);

    internal static event EventHandler<Scheme<uint>?>? SchemeChanged;
}
