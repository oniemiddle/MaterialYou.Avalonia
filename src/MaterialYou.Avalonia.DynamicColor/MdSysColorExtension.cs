using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdSysColorExtension : MarkupExtension
{
    public string? Name { get; set; }

    public MdSysColorExtension() { }
    public MdSysColorExtension(string name) => Name = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new SchemeColorObservable(Name ?? "Primary");
    }
}

internal class SchemeColorObservable : IObservable<object?>
{
    private static readonly Dictionary<string, Func<Scheme<uint>, uint>> Getters = new()
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

    private readonly string _colorName;
    private bool _disposed;

    public SchemeColorObservable(string colorName) => _colorName = colorName;

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
        var scheme = MaterialColor.GetScheme(app);
        if (scheme == null) return null;

        return Getters.TryGetValue(_colorName, out var getter) ? getter(scheme) : null;
    }
}
