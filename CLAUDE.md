# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build all
dotnet build MaterialYou.Avalonia.slnx

# Run desktop demo
dotnet run --project demo/MaterialYou.Avalonia.Demo.Desktop

# Pack a NuGet package
dotnet pack src/MaterialYou.Avalonia -o nugets
```

## Project Architecture

**MaterialYou.Avalonia** is an Avalonia theme library implementing Material Design 3 (Material You) with dynamic color support. It is a fork of Semi.Avalonia that retains the project skeleton and build infrastructure while replacing all theme content with MD3 implementations.

### Source projects (`src/`)

| Project | Description |
|---------|-------------|
| `MaterialYou.Avalonia` | Core theme library — Material 3 themed controls, design tokens, locale |
| `MaterialYou.Avalonia.DynamicColor` | Dynamic color subsystem — HCT scheme generation, markup extensions (`MdSysColor`, `MdElevation`, `MdSurface`) |
| `MaterialYou.Avalonia.MaterialSymbols` | Material Symbols icon paths as XAML resources (MVP scaffolded) |

### Demo projects (`demo/`)

- `MaterialYou.Avalonia.Demo` — Shared demo pages library (control gallery)
- `MaterialYou.Avalonia.Demo.Desktop` — Desktop launcher (Windows/macOS/Linux)

### Theme architecture

The `MaterialYouTheme` class (extends `Styles`) is the main entry point. It loads:

- **`Index.axaml`** — Root composition merging Controls, Themes, Tokens, Locale, Icons
- **`Controls/`** — Per-control style definitions (one `.axaml` per control)
- **`Themes/Light/`, `Themes/Dark/`** — Theme variant dictionaries
- **`Tokens/`** — Design tokens: `Shape.axaml` (corner radii), `TypeScale.axaml` (MD3 typography), `Motion.axaml` (curves & durations), `Variables.axaml`
- **`Locale/`** — Localized string resources for `en-US` and `zh-CN`
- **`Converters/`** — Value converters

Themes are organized as theme dictionaries via `ResourceDictionary.ThemeDictionaries`:
- `Light` / `Default` → `/Themes/Light/`
- `Dark` → `/Themes/Dark/`

### Key conventions

- **Target frameworks**: `net10.0` for all projects
- **Package versioning**: Centralized in `Directory.Packages.props` with `ManagePackageVersionsCentrally=true`
- **Avalonia version**: 12.x, nightly feed from `nuget-feed-nightly.avaloniaui.net`
- **AOT**: `IsAotCompatible=true` for all targets
- **LangVersion**: `latest` with nullable enabled
- **Commit style**: Conventional Commits (`feat:`, `fix:`, `docs:`, etc.) with lowercase descriptions, scoped by package/area
- **XAML namespace**: `https://materialyou.avalonia/dev`
- **SDK**: .NET 10.0 SDK minimum, `rollForward: latestMajor`

### Important code paths

- `MaterialYouTheme.axaml.cs` — Theme class, locale resource management
- `Tokens/Variables.axaml.cs` — Empty `ResourceDictionary` subclass used as a XAML resource key
- `DynamicColor/MaterialColor.cs` — `MaterialColor.SetScheme()` attached property
- Each control under `Controls/` has matching theme files in `Themes/Light/` and `Themes/Dark/`

### DynamicColor module

The `MaterialYou.Avalonia.DynamicColor` project provides:

- **`MaterialColor`** — Attached property for setting a `Scheme<uint>` on the application
- **`MdSysColorExtension`** — Markup extension `{MdSysColor Primary}` for MD3 system colors
- **`MdRefPaletteExtension`** — Markup extension `{MdRefPalette Primary, 60}` for reference palette tones
- **`MdElevationExtension`** — Markup extension `{MdElevation 2}` for theme-variant-aware box shadows
- **`MdSurfaceExtension`** — Markup extension `{MdSurface 2}` for surface colors with pre-blended elevation tint

The HCT color science and tonal palette generation are handled by the `MaterialColorUtilities` NuGet dependency.

## Agent skills

### Issue tracker

Issues and PRDs live as GitHub issues on the MaterialYou.Avalonia repo. Use the `gh` CLI for all operations. See `docs/agents/issue-tracker.md`.

### Triage labels

The repo uses the default label vocabulary (needs-triage, needs-info, ready-for-agent, ready-for-human, wontfix). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context repo. One CONTEXT.md at root and docs/adr/ for architectural decisions. See `docs/agents/domain.md`.
