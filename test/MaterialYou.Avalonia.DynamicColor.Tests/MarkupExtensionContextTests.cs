using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace MaterialYou.Avalonia.DynamicColor.Tests;

public class MarkupExtensionContextTests
{
    /// <summary>
    /// In Setter context (ControlTheme pseudo-class Setters), ProvideValue must return
    /// a BindingBase so Setter augments via Bind(), which subscribes to the observable
    /// and handles dynamic updates with correct binding priority and repaint triggers.
    /// </summary>
    [Fact]
    public void MdSysColor_SetterContext_ReturnsBindingBase_NotObservable()
    {
        var setter = new Setter();
        var serviceProvider = new MockServiceProvider(setter);

        var extension = new MdSysColorExtension("Primary");
        var result = extension.ProvideValue(serviceProvider);

        Assert.IsAssignableFrom<global::Avalonia.Data.BindingBase>(result);
    }

    [Fact]
    public void MdSurface_SetterContext_ReturnsBrush_NotObservable()
    {
        var setter = new Setter();
        var serviceProvider = new MockServiceProvider(setter);

        var extension = new MdSurfaceExtension(1);
        var result = extension.ProvideValue(serviceProvider);

        Assert.IsType<SolidColorBrush>(result);
    }

    [Fact]
    public void MdElevation_SetterContext_ReturnsBoxShadows_NotObservable()
    {
        var setter = new Setter();
        var serviceProvider = new MockServiceProvider(setter);

        var extension = new MdElevationExtension(1);
        var result = extension.ProvideValue(serviceProvider);

        Assert.IsType<BoxShadows>(result);
    }

    private class MockServiceProvider(Setter targetObject) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
                return new MockProvideValueTarget(targetObject);
            return null;
        }
    }

    private class MockProvideValueTarget(Setter setter) : IProvideValueTarget
    {
        public object TargetObject => setter;
        public object TargetProperty => new MockPropertyInfo();
    }

    /// <summary>
    /// Simulates Setter.Value's ClrPropertyInfo where PropertyType is object.
    /// </summary>
    private class MockPropertyInfo
    {
        public Type PropertyType => typeof(object);
    }
}
