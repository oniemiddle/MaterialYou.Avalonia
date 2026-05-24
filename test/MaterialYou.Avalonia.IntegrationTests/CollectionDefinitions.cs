// Headless Avalonia tests share the same platform instance and AvaloniaObject thread affinity.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
