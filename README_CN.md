# MaterialYou.Avalonia

[![MaterialYou.Avalonia](https://img.shields.io/nuget/v/MaterialYou.Avalonia.svg?color=red&style=flat-square)](https://www.nuget.org/packages/MaterialYou.Avalonia/)
[![MaterialYou.Avalonia](https://img.shields.io/nuget/dt/MaterialYou.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/MaterialYou.Avalonia/)

[English](./README.md)

基于 **Material Design 3 (Material You)** 设计系统的 Avalonia UI 主题库，支持动态颜色。

## 特性

- **动态颜色** — 从单个种子颜色生成完整的 Material 3 色彩方案
- **设计令牌** — MD3 排版、形状、高度、动效系统
- **主题控件** — Button、Card、TextBox、CheckBox、ToggleSwitch，带 MD3 状态层
- **主题变体** — 浅色/深色模式支持
- **Material Symbols** — 图标集成（MVP）

## 如何使用

### 安装

```bash
dotnet add package MaterialYou.Avalonia
```

在 Application 中引用 Material You 样式：

```xml
<Application
    ...
    xmlns:material="https://materialyou.avalonia/dev">
    <Application.Styles>
        <material:MaterialYouTheme />
    </Application.Styles>
</Application>
```

在代码中设置动态颜色：

```csharp
MaterialColor.SetScheme(Application.Current, new Scheme<uint>(
    seed: 0xFF6750A4  // 或检测系统主题色
));
```

这样就可以了。

## DynamicColor 包

`MaterialYou.Avalonia.DynamicColor` 包提供了在 XAML 中使用配色方案的标记扩展：

- `{MdSysColor Primary}` — MD3 系统颜色令牌
- `{MdRefPalette Primary, 60}` — 参考色板色调
- `{MdElevation 2}` — 感知主题变体的高度阴影
- `{MdSurface 2}` — 预混合高度着色的表面色

## 示例程序

```bash
dotnet run --project demo/MaterialYou.Avalonia.Demo.Desktop
```

## 致谢

- [Material Design 3](https://m3.material.io/)
- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [MaterialColorUtilities](https://github.com/Shirasagi0012/MaterialColorUtilities)
- [Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia)（原始项目架构）
