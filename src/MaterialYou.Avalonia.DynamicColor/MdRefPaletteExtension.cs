using System.Collections.Generic;
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
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = target?.TargetObject as AvaloniaObject;
        var targetProp = target?.TargetProperty as AvaloniaProperty;

        var paletteName = Palette ?? "Primary";
        var tone = Tone;

        // For Setter.Value (object type), return observable
        if (targetProp?.PropertyType == typeof(object))
            return new RefPaletteObservable(paletteName, tone);

        // For non-Avalonia targets (e.g. Setter in ControlTheme), return a mutable brush
        // that is updated in-place when the scheme changes.
        if (targetObj is not AvaloniaObject || targetProp is not AvaloniaProperty)
        {
            var app = Application.Current;
            if (app == null || MaterialColor.GetCorePalette(app) == null)
                return new SolidColorBrush(Colors.Transparent);
            var corePalette = MaterialColor.GetCorePalette(app)!;
            var color = s_paletteAccessors.TryGetValue(paletteName, out var accessor)
                ? ColorUtilities.UIntToColor(accessor(corePalette).Tone((uint)Math.Clamp(tone, 0, 100)))
                : Colors.Transparent;
            var brush = new SolidColorBrush(color);
            if (accessor != null)
            {
                var capturedAccessor = accessor;
                var capturedTone = tone;
                SchemeBrushRegistry.Register(brush, _ =>
                {
                    var cp = MaterialColor.GetCorePalette(Application.Current!);
                    return cp != null ? capturedAccessor(cp).Tone((uint)Math.Clamp(capturedTone, 0, 100)) : 0;
                });
            }
            return brush;
        }

        // For typed properties (IBrush), return SolidColorBrush
        var currentColor = GetPaletteColor(paletteName, tone);
        var resultBrush = new SolidColorBrush(currentColor);

        TrackProperty(targetObj, targetProp, paletteName, tone);

        return resultBrush;
    }

    private static void TrackProperty(AvaloniaObject? target, AvaloniaProperty? prop, string paletteName, int tone)
    {
        if (target == null || prop == null) return;

        var weakTarget = new WeakReference<AvaloniaObject>(target);

        MaterialColor.SchemeChanged += Handler;
        return;

        void Handler(object? o, Scheme<uint>? scheme)
        {
            if (weakTarget.TryGetTarget(out var t))
            {
                t.SetValue(prop, new SolidColorBrush(GetPaletteColor(paletteName, tone)));
            }
            else
            {
                MaterialColor.SchemeChanged -= Handler;
            }
        }
    }

    private static Color GetPaletteColor(string paletteName, int tone)
    {
        var app = Application.Current;
        if (app == null) return Colors.Transparent;
        var corePalette = MaterialColor.GetCorePalette(app);
        if (corePalette == null) return Colors.Transparent;

        return s_paletteAccessors.TryGetValue(paletteName, out var accessor)
            ? ColorUtilities.UIntToColor(accessor(corePalette).Tone((uint)Math.Clamp(tone, 0, 100)))
            : Colors.Transparent;
    }

    private static readonly Dictionary<string, Func<CorePalette, TonalPalette>> s_paletteAccessors = new()
    {
        [nameof(CorePalette.Primary)] = p => p.Primary,
        [nameof(CorePalette.Secondary)] = p => p.Secondary,
        [nameof(CorePalette.Tertiary)] = p => p.Tertiary,
        [nameof(CorePalette.Neutral)] = p => p.Neutral,
        [nameof(CorePalette.NeutralVariant)] = p => p.NeutralVariant,
        [nameof(CorePalette.Error)] = p => p.Error,
    };
}

internal class RefPaletteObservable(string paletteName, int tone) : IObservable<object?>
{
    private static readonly Dictionary<string, Func<CorePalette, TonalPalette>> s_paletteAccessors = new()
    {
        [nameof(CorePalette.Primary)] = p => p.Primary,
        [nameof(CorePalette.Secondary)] = p => p.Secondary,
        [nameof(CorePalette.Tertiary)] = p => p.Tertiary,
        [nameof(CorePalette.Neutral)] = p => p.Neutral,
        [nameof(CorePalette.NeutralVariant)] = p => p.NeutralVariant,
        [nameof(CorePalette.Error)] = p => p.Error,
    };

    private bool _disposed;

    public IDisposable Subscribe(IObserver<object?> observer)
    {
        Push();

        MaterialColor.SchemeChanged += Handler;

        return new DisposableAction(() =>
        {
            _disposed = true;
            MaterialColor.SchemeChanged -= Handler;
        });

        void Handler(object? o, Scheme<uint>? scheme) => Push();

        void Push()
        {
            if (_disposed) return;
            var app = Application.Current;
            if (app == null) return;
            var corePalette = MaterialColor.GetCorePalette(app);
            if (corePalette == null) return;

            var value = s_paletteAccessors.TryGetValue(paletteName, out var accessor)
                ? accessor(corePalette).Tone((uint)Math.Clamp(tone, 0, 100))
                : (uint?)null;
            observer.OnNext(value is { } u ? (object?)new SolidColorBrush(ColorUtilities.UIntToColor(u)) : null);
        }
    }
}
