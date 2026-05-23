using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdSysColorExtension(string name) : MarkupExtension
{
    public string? Name { get; set; } = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = target?.TargetObject as AvaloniaObject;
        var targetProp = target?.TargetProperty as AvaloniaProperty;

        var colorName = Name ?? "Primary";

        // If target is Setter.Value (object type), return observable for styling system
        if (targetProp?.PropertyType == typeof(object))
            return new SchemeColorObservable(colorName);

        // For typed properties (IBrush), return a SolidColorBrush and subscribe to updates
        var scheme = GetCurrentScheme();
        var color = scheme != null && _getters.TryGetValue(colorName, out var getter)
            ? ColorUtilities.UIntToColor(getter(scheme))
            : Colors.Black;

        var brush = new SolidColorBrush(color);

        // Set up dynamic updates on the target property
        TrackProperty(targetObj, targetProp, colorName);

        return brush;
    }

    private static void TrackProperty(AvaloniaObject? target, AvaloniaProperty? prop, string colorName)
    {
        if (target == null || prop == null) return;

        var weakTarget = new WeakReference<AvaloniaObject>(target);

        MaterialColor.SchemeChanged += Handler;
        return;

        void Handler(object? o, Scheme<uint>? scheme1)
        {
            if (weakTarget.TryGetTarget(out var t))
            {
                var scheme = GetCurrentScheme();
                if (scheme != null && _getters.TryGetValue(colorName, out var getter))
                {
                    var newColor = ColorUtilities.UIntToColor(getter(scheme));
                    t.SetValue(prop, new SolidColorBrush(newColor));
                }
            }
            else
            {
                MaterialColor.SchemeChanged -= Handler;
            }
        }
    }

    internal static Scheme<uint>? GetCurrentScheme()
    {
        var app = Application.Current;
        return app != null ? MaterialColor.GetScheme(app) : null;
    }

    internal static readonly Dictionary<string, Func<Scheme<uint>, uint>> _getters = new()
    {
        [nameof(Scheme<>.Primary)] = s => s.Primary,
        [nameof(Scheme<>.OnPrimary)] = s => s.OnPrimary,
        [nameof(Scheme<>.PrimaryContainer)] = s => s.PrimaryContainer,
        [nameof(Scheme<>.OnPrimaryContainer)] = s => s.OnPrimaryContainer,
        [nameof(Scheme<>.Secondary)] = s => s.Secondary,
        [nameof(Scheme<>.OnSecondary)] = s => s.OnSecondary,
        [nameof(Scheme<>.SecondaryContainer)] = s => s.SecondaryContainer,
        [nameof(Scheme<>.OnSecondaryContainer)] = s => s.OnSecondaryContainer,
        [nameof(Scheme<>.Tertiary)] = s => s.Tertiary,
        [nameof(Scheme<>.OnTertiary)] = s => s.OnTertiary,
        [nameof(Scheme<>.TertiaryContainer)] = s => s.TertiaryContainer,
        [nameof(Scheme<>.OnTertiaryContainer)] = s => s.OnTertiaryContainer,
        [nameof(Scheme<>.Error)] = s => s.Error,
        [nameof(Scheme<>.OnError)] = s => s.OnError,
        [nameof(Scheme<>.ErrorContainer)] = s => s.ErrorContainer,
        [nameof(Scheme<>.OnErrorContainer)] = s => s.OnErrorContainer,
        [nameof(Scheme<>.Background)] = s => s.Background,
        [nameof(Scheme<>.OnBackground)] = s => s.OnBackground,
        [nameof(Scheme<>.Surface)] = s => s.Surface,
        [nameof(Scheme<>.OnSurface)] = s => s.OnSurface,
        [nameof(Scheme<>.SurfaceVariant)] = s => s.SurfaceVariant,
        [nameof(Scheme<>.OnSurfaceVariant)] = s => s.OnSurfaceVariant,
        [nameof(Scheme<>.Outline)] = s => s.Outline,
        [nameof(Scheme<>.OutlineVariant)] = s => s.OutlineVariant,
        [nameof(Scheme<>.Shadow)] = s => s.Shadow,
        [nameof(Scheme<>.InverseSurface)] = s => s.InverseSurface,
        [nameof(Scheme<>.InverseOnSurface)] = s => s.InverseOnSurface,
        [nameof(Scheme<>.InversePrimary)] = s => s.InversePrimary,
        [nameof(Scheme<>.Surface1)] = s => s.Surface1,
        [nameof(Scheme<>.Surface2)] = s => s.Surface2,
        [nameof(Scheme<>.Surface3)] = s => s.Surface3,
        [nameof(Scheme<>.Surface4)] = s => s.Surface4,
        [nameof(Scheme<>.Surface5)] = s => s.Surface5,
        [nameof(Scheme<>.SurfaceDim)] = s => s.SurfaceDim,
        [nameof(Scheme<>.SurfaceBright)] = s => s.SurfaceBright,
        [nameof(Scheme<>.SurfaceContainerLowest)] = s => s.SurfaceContainerLowest,
        [nameof(Scheme<>.SurfaceContainerLow)] = s => s.SurfaceContainerLow,
        [nameof(Scheme<>.SurfaceContainer)] = s => s.SurfaceContainer,
        [nameof(Scheme<>.SurfaceContainerHigh)] = s => s.SurfaceContainerHigh,
        [nameof(Scheme<>.SurfaceContainerHighest)] = s => s.SurfaceContainerHighest,
    };
}

internal class SchemeColorObservable(string colorName) : IObservable<object?>
{
    public IDisposable Subscribe(IObserver<object?> observer)
    {
        Push();

        MaterialColor.SchemeChanged += Handler;

        return new DisposableAction(() => MaterialColor.SchemeChanged -= Handler);

        void Handler(object? o, Scheme<uint>? scheme) => Push();

        void Push()
        {
            var scheme = MdSysColorExtension.GetCurrentScheme();
            uint? value = scheme != null && MdSysColorExtension._getters.TryGetValue(colorName, out var getter)
                ? getter(scheme)
                : null;
            observer.OnNext(value is { } u ? ColorUtilities.UIntToColor(u) : null);
        }
    }
}
