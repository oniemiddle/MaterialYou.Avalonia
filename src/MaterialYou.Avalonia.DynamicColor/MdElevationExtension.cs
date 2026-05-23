using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdElevationExtension(int level) : MarkupExtension
{
    [ConstructorArgument(nameof(level))]
    public int Level { get; set; } = level;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var targetInfo = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = targetInfo?.TargetObject as AvaloniaObject;
        var targetProp = targetInfo?.TargetProperty as AvaloniaProperty;

        var level = Math.Clamp(Level, 0, 5);

        // Setter context -> return computed BoxShadows directly (no observable subscription available)
        if (targetInfo?.TargetObject is Setter)
        {
            var dark = Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;
            return level == 0 ? new BoxShadows() : new BoxShadows(GetShadow(level, dark));
        }

        // For object-typed properties (non-Setter), return observable
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
                else if (o is StyledElement se3)
                {
                    se3.ActualThemeVariantChanged -= OnThemeChanged;
                }
            }

            if (targetObj is StyledElement se2)
                se2.ActualThemeVariantChanged += OnThemeChanged;
        }

        return result;
    }

    internal static readonly (int OffsetY, int Blur, double Opacity)[] s_elevationLevels =
    [
        (0, 0, 0.0),   // Level 0 (unused sentinel)
        (1, 3, 0.30),  // Level 1
        (2, 6, 0.30),  // Level 2
        (4, 10, 0.30), // Level 3
        (6, 14, 0.30), // Level 4
        (8, 20, 0.30), // Level 5
    ];

    private static BoxShadow GetShadow(int level, bool isDark)
    {
        if (level == 0) return default;

        var entry = s_elevationLevels[level];
        var alpha = isDark ? (byte)(entry.Opacity * 0.35 * 255) : (byte)(entry.Opacity * 255);
        return new BoxShadow
        {
            OffsetX = 0,
            OffsetY = entry.OffsetY,
            Blur = entry.Blur,
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

            var entry = MdElevationExtension.s_elevationLevels[level];
            var alpha = isDark ? (byte)(entry.Opacity * 0.35 * 255) : (byte)(entry.Opacity * 255);
            observer.OnNext(new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = entry.OffsetY,
                Blur = entry.Blur,
                Color = Color.FromArgb(alpha, 0, 0, 0)
            }));
        }
    }
}
