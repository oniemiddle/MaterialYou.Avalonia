using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdSysColorExtension(string name) : MarkupExtension
{
    [ConstructorArgument(nameof(name))]
    public string? Name { get; set; } = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var targetInfo = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = targetInfo?.TargetObject as AvaloniaObject;
        var targetProp = targetInfo?.TargetProperty as AvaloniaProperty;

        var colorName = Name ?? "Primary";

        // Setter context (Style/ControlTheme) -> return BindingBase so Setter augments
        // via Bind(), which subscribes to the observable and handles dynamic updates with
        // correct binding priority and automatic repaint triggers.
        if (targetInfo?.TargetObject is Setter)
        {
            if (_getters.ContainsKey(colorName))
                return new SchemeColorBindingObservable(colorName).ToBinding();
            return new SolidColorBrush(Colors.Black);
        }

        // For object-typed properties (non-Setter), return observable
        if (targetProp?.PropertyType == typeof(object))
            return new SchemeColorObservable(colorName);

        // For non-Avalonia targets (other non-visual-tree contexts), return registered brush
        if (targetObj is not AvaloniaObject || targetProp is not AvaloniaProperty)
        {
            if (_getters.TryGetValue(colorName, out var getter))
            {
                var scheme = GetCurrentScheme();
                var initialColor = scheme != null
                    ? ColorUtilities.UIntToColor(getter(scheme))
                    : Colors.Black;
                var brush = new SolidColorBrush(initialColor);
                SchemeBrushRegistry.Register(brush, s => getter(s));
                return brush;
            }
            return new SolidColorBrush(Colors.Black);
        }

        // For typed properties (IBrush), return a SolidColorBrush and subscribe to updates
        var currentScheme = GetCurrentScheme();
        var currentColor = currentScheme != null && _getters.TryGetValue(colorName, out var currentGetter)
            ? ColorUtilities.UIntToColor(currentGetter(currentScheme))
            : Colors.Black;

        var resultBrush = new SolidColorBrush(currentColor);

        TrackProperty(targetObj, targetProp, colorName);

        return resultBrush;
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
                    if (t.GetValue(prop) is SolidColorBrush existing)
                    {
                        existing.Color = newColor;
                        if (t is Visual v) v.InvalidateVisual();
                    }
                    else
                        t.SetCurrentValue(prop, new SolidColorBrush(newColor));
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

/// <summary>
/// IObservable for Setter context. Emits SolidColorBrush when scheme changes.
/// Used with ToBinding() to let Setter subscribe via the BindingBase infrastructure
/// which triggers property change → automatic repaint.
/// </summary>
internal class SchemeColorBindingObservable(string colorName) : IObservable<IBrush?>
{
    public IDisposable Subscribe(IObserver<IBrush?> observer)
    {
        Push();

        MaterialColor.SchemeChanged += Handler;

        return new DisposableAction(() => MaterialColor.SchemeChanged -= Handler);

        void Handler(object? o, Scheme<uint>? scheme) => Push();

        void Push()
        {
            var scheme = MdSysColorExtension.GetCurrentScheme();
            if (scheme != null && MdSysColorExtension._getters.TryGetValue(colorName, out var getter))
            {
                var color = ColorUtilities.UIntToColor(getter(scheme));
                observer.OnNext(new SolidColorBrush(color));
            }
        }
    }
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
            observer.OnNext(value is { } u ? (object?)new SolidColorBrush(ColorUtilities.UIntToColor(u)) : null);
        }
    }
}
