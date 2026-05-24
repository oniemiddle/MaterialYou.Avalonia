using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;
using MaterialColorUtilities.Schemes;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdSurfaceExtension(int level) : MarkupExtension
{
    [ConstructorArgument(nameof(level))]
    public int Level { get; set; } = level;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var targetInfo = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = targetInfo?.TargetObject as AvaloniaObject;
        var targetProp = targetInfo?.TargetProperty as AvaloniaProperty;

        var level = Math.Clamp(Level, 0, 5);

        // Setter context -> return mutable brush registered for in-place updates
        if (targetInfo?.TargetObject is Setter)
        {
            var color = GetSurfaceColor(level);
            var brush = new SolidColorBrush(color);
            SchemeBrushRegistry.Register(brush, s => SurfaceAccessor(level, s));
            return brush;
        }

        // For object-typed properties (non-Setter), return observable
        if (targetProp?.PropertyType == typeof(object))
            return new SurfaceObservable(level);

        // For non-Avalonia targets (other non-visual-tree contexts), return registered brush
        if (targetObj is not AvaloniaObject || targetProp is not AvaloniaProperty)
        {
            var color = GetSurfaceColor(level);
            var brush = new SolidColorBrush(color);
            SchemeBrushRegistry.Register(brush, s => SurfaceAccessor(level, s));
            return brush;
        }

        // For typed properties (IBrush), return SolidColorBrush
        var currentColor = GetSurfaceColor(level);
        var resultBrush = new SolidColorBrush(currentColor);

        TrackProperty(targetObj, targetProp, level);

        return resultBrush;
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
                var newColor = GetSurfaceColor(level);
                if (t.GetValue(prop) is SolidColorBrush existing)
                {
                    existing.Color = newColor;
                    if (t is Visual v) v.InvalidateVisual();
                }
                else
                    t.SetCurrentValue(prop, new SolidColorBrush(newColor));
            }
            else
            {
                MaterialColor.SchemeChanged -= Handler;
            }
        }
    }

    private static uint SurfaceAccessor(int level, Scheme<uint> scheme) => level switch
    {
        0 => scheme.Surface,
        1 => scheme.Surface1,
        2 => scheme.Surface2,
        3 => scheme.Surface3,
        4 => scheme.Surface4,
        5 => scheme.Surface5,
        _ => scheme.Surface,
    };

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
            observer.OnNext(new SolidColorBrush(ColorUtilities.UIntToColor(s_surfaceAccessors[level](scheme))));
        }
    }
}
