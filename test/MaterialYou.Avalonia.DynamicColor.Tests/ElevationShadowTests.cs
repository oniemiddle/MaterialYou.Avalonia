using Avalonia.Media;

namespace MaterialYou.Avalonia.DynamicColor.Tests;

public class ElevationShadowTests
{
    public static IEnumerable<object[]> ShadowLevelData => Enumerable.Range(0, 6).Select(i => new object[] { i });

    [Theory]
    [MemberData(nameof(ShadowLevelData))]
    public void ElevationObservable_PushesBoxShadows(int level)
    {
        var observable = new ElevationObservable(level, null);
        object? result = null;

        using (observable.Subscribe(new DelegateObserver(v => result = v)))
        {
            Assert.NotNull(result);
            Assert.IsType<BoxShadows>(result);
        }
    }
}

internal class DelegateObserver(Action<object?> onNext) : IObserver<object?>
{
    public void OnCompleted() { }
    public void OnError(Exception error) { }
    public void OnNext(object? value) => onNext(value);
}
