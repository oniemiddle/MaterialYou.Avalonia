using Avalonia.Markup.Xaml;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdSurfaceExtension(int level) : MarkupExtension
{
    public int Level { get; set; } = level;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = target?.TargetObject as AvaloniaObject;
        var targetProp = target?.TargetProperty as AvaloniaProperty;

        var level = Math.Clamp(Level, 0, 5);

        // For Setter.Value (object type), return observable
        if (targetProp?.PropertyType == typeof(object))
            return new SurfaceObservable(level);

        // For typed properties (IBrush), return SolidColorBrush
        var color = GetSurfaceColor(level);
        var brush = new SolidColorBrush(color);

        TrackProperty(targetObj, targetProp, level);

        return brush;
    }

    private static void TrackProperty(AvaloniaObject? target, AvaloniaProperty? prop, int level)
    {
        if (target == null || prop == null) return;

        var weakTarget = new WeakReference<AvaloniaObject>(target);

        MaterialColor.SchemeChanged += Handler;
        return;

        void Handler(object? o, Scheme<uint>? scheme)
        {
            if (weakTarget.TryGetTarget(out var t))
            {
                t.SetValue(prop, new SolidColorBrush(GetSurfaceColor(level)));
            }
            else
            {
                MaterialColor.SchemeChanged -= Handler;
            }
        }
    }

    private static Color GetSurfaceColor(int level)
    {
        var app = Application.Current;
        if (app == null) return Colors.Transparent;
        var scheme = MaterialColor.GetScheme(app);
        if (scheme == null) return Colors.Transparent;

        return level switch
        {
            0 => ColorUtilities.UIntToColor(scheme.Surface),
            1 => ColorUtilities.UIntToColor(scheme.Surface1),
            2 => ColorUtilities.UIntToColor(scheme.Surface2),
            3 => ColorUtilities.UIntToColor(scheme.Surface3),
            4 => ColorUtilities.UIntToColor(scheme.Surface4),
            5 => ColorUtilities.UIntToColor(scheme.Surface5),
            _ => Colors.Transparent,
        };
    }
}

internal class SurfaceObservable(int level) : IObservable<object?>
{
    private static readonly Func<Scheme<uint>, uint>[] s_surfaceAccessors =
    [
        s => s.Surface,
        s => s.Surface1,
        s => s.Surface2,
        s => s.Surface3,
        s => s.Surface4,
        s => s.Surface5,
    ];

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
            var scheme = MaterialColor.GetScheme(app);
            if (scheme == null) return;
            observer.OnNext(ColorUtilities.UIntToColor(s_surfaceAccessors[level](scheme)));
        }
    }
}
