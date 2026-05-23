using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace MaterialYou.Avalonia.DynamicColor;

public class MdElevationExtension : MarkupExtension
{
    public int Level { get; set; }

    public MdElevationExtension() { }
    public MdElevationExtension(int level) => Level = level;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var targetObj = target?.TargetObject as AvaloniaObject;
        return new ElevationObservable(Math.Clamp(Level, 0, 5), targetObj);
    }
}

internal class ElevationObservable : IObservable<object?>
{
    private readonly int _level;
    private readonly AvaloniaObject? _target;
    private bool _disposed;

    public ElevationObservable(int level, AvaloniaObject? target)
    {
        _level = level;
        _target = target;
    }

    public IDisposable Subscribe(IObserver<object?> observer)
    {
        void Push()
        {
            if (_disposed) return;
            observer.OnNext(GetCurrentValue());
        }

        Push();

        EventHandler? onThemeChanged = null;
        if (_target is StyledElement se)
            se.ActualThemeVariantChanged += onThemeChanged = (_, _) => Push();

        return new DisposableAction(() =>
        {
            _disposed = true;
            if (onThemeChanged != null && _target is StyledElement se2)
                se2.ActualThemeVariantChanged -= onThemeChanged;
        });
    }

    private object GetCurrentValue()
    {
        if (_level == 0) return new BoxShadows();

        var isDark = _target is StyledElement se &&
            se.ActualThemeVariant == ThemeVariant.Dark;

        return new BoxShadows(GetShadow(_level, isDark));
    }

    private static BoxShadow GetShadow(int level, bool isDark)
    {
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
