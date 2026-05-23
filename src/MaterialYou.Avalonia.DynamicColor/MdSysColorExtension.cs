using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdSysColorExtension : MarkupExtension
{
    public string? Name { get; set; }

    public MdSysColorExtension() { }
    public MdSysColorExtension(string name) => Name = name;

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
        var color = scheme != null && Getters.TryGetValue(colorName, out var getter)
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

        EventHandler<Scheme<uint>?>? handler = null;
        handler = (_, _) =>
        {
            if (weakTarget.TryGetTarget(out var t))
            {
                var scheme = GetCurrentScheme();
                if (scheme != null && Getters.TryGetValue(colorName, out var getter))
                {
                    var newColor = ColorUtilities.UIntToColor(getter(scheme));
                    t.SetValue(prop, new SolidColorBrush(newColor));
                }
            }
            else
            {
                MaterialColor.SchemeChanged -= handler;
            }
        };
        MaterialColor.SchemeChanged += handler;
    }

    internal static Scheme<uint>? GetCurrentScheme()
    {
        var app = Application.Current;
        return app != null ? MaterialColor.GetScheme(app) : null;
    }

    internal static readonly Dictionary<string, Func<Scheme<uint>, uint>> Getters = new()
    {
        [nameof(Scheme<uint>.Primary)] = s => s.Primary,
        [nameof(Scheme<uint>.OnPrimary)] = s => s.OnPrimary,
        [nameof(Scheme<uint>.PrimaryContainer)] = s => s.PrimaryContainer,
        [nameof(Scheme<uint>.OnPrimaryContainer)] = s => s.OnPrimaryContainer,
        [nameof(Scheme<uint>.Secondary)] = s => s.Secondary,
        [nameof(Scheme<uint>.OnSecondary)] = s => s.OnSecondary,
        [nameof(Scheme<uint>.SecondaryContainer)] = s => s.SecondaryContainer,
        [nameof(Scheme<uint>.OnSecondaryContainer)] = s => s.OnSecondaryContainer,
        [nameof(Scheme<uint>.Tertiary)] = s => s.Tertiary,
        [nameof(Scheme<uint>.OnTertiary)] = s => s.OnTertiary,
        [nameof(Scheme<uint>.TertiaryContainer)] = s => s.TertiaryContainer,
        [nameof(Scheme<uint>.OnTertiaryContainer)] = s => s.OnTertiaryContainer,
        [nameof(Scheme<uint>.Error)] = s => s.Error,
        [nameof(Scheme<uint>.OnError)] = s => s.OnError,
        [nameof(Scheme<uint>.ErrorContainer)] = s => s.ErrorContainer,
        [nameof(Scheme<uint>.OnErrorContainer)] = s => s.OnErrorContainer,
        [nameof(Scheme<uint>.Background)] = s => s.Background,
        [nameof(Scheme<uint>.OnBackground)] = s => s.OnBackground,
        [nameof(Scheme<uint>.Surface)] = s => s.Surface,
        [nameof(Scheme<uint>.OnSurface)] = s => s.OnSurface,
        [nameof(Scheme<uint>.SurfaceVariant)] = s => s.SurfaceVariant,
        [nameof(Scheme<uint>.OnSurfaceVariant)] = s => s.OnSurfaceVariant,
        [nameof(Scheme<uint>.Outline)] = s => s.Outline,
        [nameof(Scheme<uint>.OutlineVariant)] = s => s.OutlineVariant,
        [nameof(Scheme<uint>.Shadow)] = s => s.Shadow,
        [nameof(Scheme<uint>.InverseSurface)] = s => s.InverseSurface,
        [nameof(Scheme<uint>.InverseOnSurface)] = s => s.InverseOnSurface,
        [nameof(Scheme<uint>.InversePrimary)] = s => s.InversePrimary,
        [nameof(Scheme<uint>.Surface1)] = s => s.Surface1,
        [nameof(Scheme<uint>.Surface2)] = s => s.Surface2,
        [nameof(Scheme<uint>.Surface3)] = s => s.Surface3,
        [nameof(Scheme<uint>.Surface4)] = s => s.Surface4,
        [nameof(Scheme<uint>.Surface5)] = s => s.Surface5,
        [nameof(Scheme<uint>.SurfaceDim)] = s => s.SurfaceDim,
        [nameof(Scheme<uint>.SurfaceBright)] = s => s.SurfaceBright,
        [nameof(Scheme<uint>.SurfaceContainerLowest)] = s => s.SurfaceContainerLowest,
        [nameof(Scheme<uint>.SurfaceContainerLow)] = s => s.SurfaceContainerLow,
        [nameof(Scheme<uint>.SurfaceContainer)] = s => s.SurfaceContainer,
        [nameof(Scheme<uint>.SurfaceContainerHigh)] = s => s.SurfaceContainerHigh,
        [nameof(Scheme<uint>.SurfaceContainerHighest)] = s => s.SurfaceContainerHighest,
    };
}

internal class SchemeColorObservable : IObservable<object?>
{
    private readonly string _colorName;

    public SchemeColorObservable(string colorName) => _colorName = colorName;

    public IDisposable Subscribe(IObserver<object?> observer)
    {
        void Push()
        {
            var scheme = MdSysColorExtension.GetCurrentScheme();
            uint? value = scheme != null && MdSysColorExtension.Getters.TryGetValue(_colorName, out var getter)
                ? getter(scheme)
                : null;
            observer.OnNext(value is uint u ? (object)ColorUtilities.UIntToColor(u) : null);
        }

        Push();

        EventHandler<Scheme<uint>?>? handler = null;
        handler = (_, _) => Push();
        MaterialColor.SchemeChanged += handler;

        return new DisposableAction(() => MaterialColor.SchemeChanged -= handler);
    }
}
