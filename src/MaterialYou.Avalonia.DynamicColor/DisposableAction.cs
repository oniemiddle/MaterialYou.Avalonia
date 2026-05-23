using System.Threading;

namespace MaterialYou.Avalonia.DynamicColor;

internal sealed class DisposableAction(Action action) : IDisposable
{
    private int _disposed;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            action();
    }
}
