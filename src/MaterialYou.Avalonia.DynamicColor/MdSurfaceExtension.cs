using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdSurfaceExtension : MarkupExtension
{
    public int Level { get; set; }

    public MdSurfaceExtension() { }
    public MdSurfaceExtension(int level) => Level = level;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new SurfaceObservable(Math.Clamp(Level, 0, 5));
    }
}

internal class SurfaceObservable : IObservable<object?>
{
    private static readonly Func<Scheme<uint>, uint>[] SurfaceAccessors =
    [
        s => s.Surface,
        s => s.Surface1,
        s => s.Surface2,
        s => s.Surface3,
        s => s.Surface4,
        s => s.Surface5,
    ];

    private readonly int _level;
    private bool _disposed;

    public SurfaceObservable(int level) => _level = level;

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

        return SurfaceAccessors[_level](scheme);
    }
}
