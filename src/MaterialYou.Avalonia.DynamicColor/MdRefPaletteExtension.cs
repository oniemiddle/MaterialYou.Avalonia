using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml;
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
        return new RefPaletteObservable(Palette ?? "Primary", Tone);
    }
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
            var value = GetCurrentValue();
            observer.OnNext(value is uint u ? (object)ColorUtilities.UIntToColor(u) : value);
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

    private object? GetCurrentValue()
    {
        var app = Application.Current;
        if (app == null) return null;

        var corePalette = MaterialColor.GetCorePalette(app);
        if (corePalette == null) return null;

        return PaletteAccessors.TryGetValue(_paletteName, out var accessor)
            ? accessor(corePalette).Tone((uint)_tone)
            : null;
    }
}
