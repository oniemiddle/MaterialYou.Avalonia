# MaterialYou.Avalonia

[![MaterialYou.Avalonia](https://img.shields.io/nuget/v/MaterialYou.Avalonia.svg?color=red&style=flat-square)](https://www.nuget.org/packages/MaterialYou.Avalonia/)
[![MaterialYou.Avalonia](https://img.shields.io/nuget/dt/MaterialYou.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/MaterialYou.Avalonia/)

[中文](./README_CN.md)

Avalonia theme library implementing **Material Design 3 (Material You)** design system with dynamic color support.

## Features

- **Dynamic Color** — Generate a complete Material 3 color scheme from a single seed color
- **Design Tokens** — MD3 typography scale, shape system, elevation, and motion
- **Themed Controls** — Button, Card, TextBox, CheckBox, ToggleSwitch with MD3 state layers
- **Theme Variants** — Light and Dark mode support
- **Material Symbols** — Icon integration (MVP)

## How to Use

### Installation

```bash
dotnet add package MaterialYou.Avalonia
```

Include Material You styles in your Application:

```xml
<Application
    ...
    xmlns:material="https://materialyou.avalonia/dev">
    <Application.Styles>
        <material:MaterialYouTheme />
    </Application.Styles>
</Application>
```

Set up dynamic color in code-behind:

```csharp
MaterialColor.SetScheme(Application.Current, new Scheme<uint>(
    seed: 0xFF6750A4  // or detect system accent color
));
```

That's all.

## DynamicColor

The `MaterialYou.Avalonia.DynamicColor` package provides markup extensions for consuming scheme colors in XAML:

- `{MdSysColor Primary}` — MD3 system color token
- `{MdRefPalette Primary, 60}` — reference palette tone
- `{MdElevation 2}` — theme-variant-aware elevation shadow
- `{MdSurface 2}` — surface with pre-blended elevation tint

## Demo

```bash
dotnet run --project demo/MaterialYou.Avalonia.Demo.Desktop
```

## Credits

- [Material Design 3](https://m3.material.io/)
- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [MaterialColorUtilities](https://github.com/Shirasagi0012/MaterialColorUtilities)
- [Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia) (original project architecture)
