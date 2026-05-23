// AvaloniaObject-derived types (SolidColorBrush, etc.) have thread affinity.
// Tests that modify them must run serially to avoid cross-thread access violations.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
