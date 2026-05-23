using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdElevationExtension(int level) : MarkupExtension
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
            return new ElevationObservable(level, targetObj);

        // For typed properties (BoxShadows), compute and return directly
        var isDark = targetObj is StyledElement se &&
            se.ActualThemeVariant == ThemeVariant.Dark;

        var result = level == 0 ? new BoxShadows() : new BoxShadows(GetShadow(level, isDark));

        // Subscribe to theme variant changes to update the property
        if (targetObj != null && targetProp != null && level > 0)
        {
            var weakTarget = new WeakReference<AvaloniaObject>(targetObj);

            void OnThemeChanged(object? o, EventArgs eventArgs)
            {
                if (weakTarget.TryGetTarget(out var t) && t is StyledElement se2)
                {
                    var dark = se2.ActualThemeVariant == ThemeVariant.Dark;
                    t.SetValue(targetProp, new BoxShadows(GetShadow(level, dark)));
                }
            }

            if (targetObj is StyledElement se2)
                se2.ActualThemeVariantChanged += OnThemeChanged;
        }

        return result;
    }

    private static BoxShadow GetShadow(int level, bool isDark)
    {
        if (level == 0) return default;

        var (y, blur, opacity) = level switch
        {
            1 => (1, 3, 0.30),
            2 => (2, 6, 0.30),
            3 => (4, 10, 0.30),
            4 => (6, 14, 0.30),
            5 => (8, 20, 0.30),
            _ => (0, 0, 0.0),
        };
        var alpha = isDark ? (byte)(opacity * 0.35 * 255) : (byte)(opacity * 255);
        return new BoxShadow
        {
            OffsetX = 0,
            OffsetY = y,
            Blur = blur,
            Color = Color.FromArgb(alpha, 0, 0, 0)
        };
    }
}

internal class ElevationObservable(int level, AvaloniaObject? target) : IObservable<object?>
{
    private bool _disposed;

    public IDisposable Subscribe(IObserver<object?> observer)
    {
        Push();

        if (target is StyledElement se)
        {
            se.ActualThemeVariantChanged += OnThemeChanged;
        }

        return new DisposableAction(() =>
        {
            _disposed = true;
            if (target is StyledElement se2)
                se2.ActualThemeVariantChanged -= OnThemeChanged;
        });
        
        void OnThemeChanged(object? o, EventArgs eventArgs) => Push();

        void Push()
        {
            if (_disposed) return;
            if (level == 0)
            {
                observer.OnNext(new BoxShadows());
                return;
            }

            var isDark = target is StyledElement se1 &&
                         se1.ActualThemeVariant == ThemeVariant.Dark;

            var (y, blur, opacity) = level switch
            {
                1 => (1, 3, 0.30),
                2 => (2, 6, 0.30),
                3 => (4, 10, 0.30),
                4 => (6, 14, 0.30),
                5 => (8, 20, 0.30),
                _ => (0, 0, 0.0),
            };
            var alpha = isDark ? (byte)(opacity * 0.35 * 255) : (byte)(opacity * 255);
            observer.OnNext(new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = y,
                Blur = blur,
                Color = Color.FromArgb(alpha, 0, 0, 0)
            }));
        }
    }
}
