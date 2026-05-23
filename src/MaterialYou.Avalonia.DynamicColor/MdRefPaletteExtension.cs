using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdRefPaletteExtension : MarkupExtension
{
    public string? Palette { get; set; }
    public int Tone { get; set; } = 50;

    public MdRefPaletteExtension() { }
    public MdRefPaletteExtension(string palette) => Palette = palette;
    public MdRefPaletteExtension(string palette, int tone) { Palette = palette; Tone = tone; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = target?.TargetObject as AvaloniaObject;
        var targetProp = target?.TargetProperty as AvaloniaProperty;

        var paletteName = Palette ?? "Primary";
        var tone = Tone;

        // For Setter.Value (object type), return observable
        if (targetProp?.PropertyType == typeof(object))
            return new RefPaletteObservable(paletteName, tone);

        // For typed properties (IBrush), return SolidColorBrush
        var color = GetPaletteColor(paletteName, tone);
        var brush = new SolidColorBrush(color);

        TrackProperty(targetObj, targetProp, paletteName, tone);

        return brush;
    }

    private static void TrackProperty(AvaloniaObject? target, AvaloniaProperty? prop, string paletteName, int tone)
    {
        if (target == null || prop == null) return;

        var weakTarget = new WeakReference<AvaloniaObject>(target);

        EventHandler<Scheme<uint>?>? handler = null;
        handler = (_, _) =>
        {
            if (weakTarget.TryGetTarget(out var t))
            {
                t.SetValue(prop, new SolidColorBrush(GetPaletteColor(paletteName, tone)));
            }
            else
            {
                MaterialColor.SchemeChanged -= handler;
            }
        };
        MaterialColor.SchemeChanged += handler;
    }

    private static Color GetPaletteColor(string paletteName, int tone)
    {
        var app = Application.Current;
        if (app == null) return Colors.Transparent;
        var corePalette = MaterialColor.GetCorePalette(app);
        if (corePalette == null) return Colors.Transparent;

        return PaletteAccessors.TryGetValue(paletteName, out var accessor)
            ? ColorUtilities.UIntToColor(accessor(corePalette).Tone((uint)tone))
            : Colors.Transparent;
    }

    private static readonly Dictionary<string, Func<CorePalette, TonalPalette>> PaletteAccessors = new()
    {
        [nameof(CorePalette.Primary)] = p => p.Primary,
        [nameof(CorePalette.Secondary)] = p => p.Secondary,
        [nameof(CorePalette.Tertiary)] = p => p.Tertiary,
        [nameof(CorePalette.Neutral)] = p => p.Neutral,
        [nameof(CorePalette.NeutralVariant)] = p => p.NeutralVariant,
        [nameof(CorePalette.Error)] = p => p.Error,
    };
}

internal class RefPaletteObservable : IObservable<object?>
{
    private static readonly Dictionary<string, Func<CorePalette, TonalPalette>> PaletteAccessors = new()
    {
        [nameof(CorePalette.Primary)] = p => p.Primary,
        [nameof(CorePalette.Secondary)] = p => p.Secondary,
        [nameof(CorePalette.Tertiary)] = p => p.Tertiary,
        [nameof(CorePalette.Neutral)] = p => p.Neutral,
        [nameof(CorePalette.NeutralVariant)] = p => p.NeutralVariant,
        [nameof(CorePalette.Error)] = p => p.Error,
    };

    private readonly string _paletteName;
    private readonly int _tone;
    private bool _disposed;

    public RefPaletteObservable(string paletteName, int tone)
    {
        _paletteName = paletteName;
        _tone = tone;
    }

    public IDisposable Subscribe(IObserver<object?> observer)
    {
        void Push()
        {
            if (_disposed) return;
            var app = Application.Current;
            if (app == null) return;
            var corePalette = MaterialColor.GetCorePalette(app);
            if (corePalette == null) return;

            var value = PaletteAccessors.TryGetValue(_paletteName, out var accessor)
                ? accessor(corePalette).Tone((uint)_tone)
                : (uint?)null;
            observer.OnNext(value is uint u ? (object)ColorUtilities.UIntToColor(u) : null);
        }

        Push();

        EventHandler<Scheme<uint>?>? handler = null;
        handler = (_, _) => Push();
        MaterialColor.SchemeChanged += handler;

        return new DisposableAction(() =>
        {
            _disposed = true;
            MaterialColor.SchemeChanged -= handler;
        });
    }
}
