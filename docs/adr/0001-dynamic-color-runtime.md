# 0001: Runtime Dynamic Color via HCT + Monet Algorithm

We use runtime HCT-based color generation (Material Design 3 Dynamic Color / Monet) instead of compile-time static XAML color palettes. The `MaterialColorUtilities` NuGet package provides the HCT color space, tonal palette generation, and scheme mapping; our project (`MaterialYou.Avalonia.DynamicColor`) wraps this with Avalonia markup extensions (`MdSysColor`, `MdRefPalette`, `MdElevation`, `MdSurface`) and a `MaterialColor.Scheme` attached property that feeds scheme colors into the resource system. Scheme generation is triggered from code-behind, not XAML, to enable system accent color detection and runtime seed color changes.

## Considered Options

- **Static XAML palettes (Semi's approach)** — pre-defined Light/Dark palette files. Simple and predictable, but cannot implement Material 3's core value proposition: dynamic personalization from a seed color. Would require the user to manually define and switch between XAML palette variants.
- **Runtime HCT generation** — the chosen approach. Adds a dependency and a C# integration layer, but enables genuine Monet-style theming where changing one seed color produces a full accessible 5-role color scheme.

## Consequences

- Theme colors are not visible at XAML compile time; control templates must reference colors via markup extensions (`MdSysColor`, `MdRefPalette`) rather than `StaticResource` or `DynamicResource` pointing to predefined brushes.
- The `MaterialYouTheme` (the `Styles` entry point) cannot embed the scheme — it is a pure XAML control template collection. Users must call `MaterialColor.SetScheme()` in their code-behind.
- `MaterialColorUtilities` NuGet becomes a permanent dependency of the DynamicColor project.
- The HCT/Scheme algorithm adaptation from `Shirasagi0012/MaterialColorUtilities` (Apache-2.0) must be properly attributed in the DynamicColor project.
- System color detection (e.g., `UISettings.GetColorValue`) is a platform-specific concern handled in the demo/calling code, not the library itself.
