# PRD: MaterialYou.Avalonia — MVP Implementation

## Problem Statement

Avalonia UI lacks a production-quality Material Design 3 (Material You) theme library. Existing theme libraries for Avalonia either implement Material Design 2 or use alternative design systems (like Semi Design). Developers who want to build Avalonia apps with Material You's dynamic color system, accessible design tokens, and modern component aesthetics are forced to hand-roll every control template and color scheme.

The Semi.Avalonia project provides a mature architecture for an Avalonia theme library (control templating, design tokens, theme variants, localization, build infrastructure) but its visual design and color system belong to Semi Design, not Material 3.

## Solution

Fork Semi.Avalonia, retain its project skeleton and build infrastructure, and replace all theme content with Material 3 implementations. The result — `MaterialYou.Avalonia` — is an Avalonia UI theme library that implements the Material Design 3 specification with runtime dynamic color, accessible design tokens, and proper MD3 components.

## User Stories

1. As an Avalonia developer, I want to apply a Material 3 theme to my application by adding a single `Styles` entry and one line of code-behind, so that my app instantly adopts the MD3 design language.
2. As an end user, I want the UI to respect my system Light/Dark preference, so that I get a comfortable viewing experience in any lighting condition.
3. As an end user, I want the UI to use my system accent color when available, so that my apps feel personalized and consistent with my OS theme.
4. As a developer, I want to supply a custom seed color and have the entire color scheme (5 roles × Light/Dark) generated automatically, so that I can brand my app without manually defining 50+ color resources.
5. As a developer, I want to change the seed color at runtime and see all controls update immediately, so that I can implement in-app theme customization.
6. As a developer, I want to use the standard Material 3 Button variants (Filled, Tonal, Outlined, Text) via simple XAML attributes, so that I can build UIs following MD3 guidelines.
7. As a developer, I want Cards with proper MD3 elevation and surface tint that respond to Light/Dark mode correctly, so that my card layouts have the correct visual hierarchy.
8. As a developer, I want Text Fields (filled and outlined variants) that follow MD3 shape, typography, and state layer conventions, so that text input feels native to Material You.
9. As a developer, I want CheckBox and Switch controls styled to the MD3 specification, so that selection controls are visually consistent with the rest of the theme.
10. As a developer, I want to reference Material 3 color tokens (`Primary`, `OnPrimaryContainer`, etc.) in my own XAML, so that I can build custom layouts that stay on-theme.
11. As a developer, I want typography to follow the MD3 type scale (Display, Headline, Title, Body, Label), so that text hierarchy is consistent without manual font-size management.
12. As a developer, I want controls to use correct MD3 state layer feedback (hover/focus/pressed) so that interactive elements communicate their state without jarring color changes.
13. As a developer, I want to use Material Symbols icons in my app via simple XAML references, so that I have access to Google's complete icon library.

## Implementation Decisions

### Module 1: Build Infrastructure Renovation

The project skeleton is retained from Semi.Avalonia. Every Semi.Avalonia reference (file names, namespaces, project names, assembly attributes, XML namespaces, NuGet package IDs) is renamed to MaterialYou.Avalonia. All Semi-specific source content is removed or replaced.

**Files to rename:** `Semi.Avalonia.slnx` → `MaterialYou.Avalonia.slnx`, all `.csproj` files under `src/` and `demo/`.

**Namespaces:** `Semi.Avalonia.*` → `MaterialYou.Avalonia.*`.

**XML namespace:** `https://irihi.tech/semi` → `https://materialyou.avalonia/dev` (registered via `XmlnsDefinition` in AssemblyInfo.cs).

**Dependencies added:** `MaterialColorUtilities` NuGet package pinned at the latest stable version.

**Unused projects removed:** `Semi.Avalonia.ColorPicker`, `Semi.Avalonia.DataGrid`, `Semi.Avalonia.TreeDataGrid` (not part of MVP). Unused demo launchers (`Web`, `Android`, `DRM`) retained as directory stubs or removed — Desktop-only for MVP.

**Locale reduced:** 16 locales trimmed to `en-US` and `zh-CN`.

### Module 2: DynamicColor Subsystem (Deep Module)

A new `MaterialYou.Avalonia.DynamicColor` project adapts the Avalonia integration layer from `Shirasagi0012/MaterialColorUtilities` (Apache-2.0). This is a deep module: all HCT color science, tonal palette generation, and scheme mapping are handled by the `MaterialColorUtilities` NuGet dependency; the DynamicColor project provides only the Avalonia-native surface.

**Public API surface:**

```csharp
// Attached property — entry point
public static class MaterialColor
{
    public static readonly AttachedProperty<Scheme<uint>> SchemeProperty;
    public static void SetScheme(AvaloniaObject target, Scheme<uint> scheme);
    public static Scheme<uint> GetScheme(AvaloniaObject target);
}

// Markup extensions — consumption
public class MdSysColorExtension    // {MdSysColor Primary}
public class MdRefPaletteExtension  // {MdRefPalette Primary, 60}
public class MdElevationExtension   // {MdElevation 2} — theme-variant-aware BoxShadow
public class MdSurfaceExtension     // {MdSurface 2} — surface + elevation tint pre-blended
```

**Key behaviors:**
- `MdElevation` and `MdSurface` are theme-variant-aware: in Dark mode, shadows are reduced/eliminated; surface tint is adjusted accordingly.
- `MaterialColor` listens to `ActualThemeVariantChanged` on the target element, re-resolving all color bindings when the theme switches.
- A `SystemColorHelper` static class provides a platform-agnostic helper to detect the system accent color (`UISettings.GetColorValue` on Windows), with a fallback seed color (`0xFF6750A4` — Material You default purple).
- Scheme changed at runtime → all consuming markup extensions update automatically.

### Module 3: Design Tokens

All token resources are XAML files under `Tokens/`:

- **`Tokens/Variables.axaml`** — sizing, spacing, border thickness (inherited from Semi's approach but values adjusted to MD3 spec).
- **`Tokens/Shape.axaml`** — 5 `CornerRadius` resources (None=0, Small=4, Medium=8, Large=12, Full=16).
- **`Tokens/TypeScale.axaml`** — Pre-composed `Style` resources for each MD3 level: `DisplayLarge`/`DisplayMedium`/`DisplaySmall`, `HeadlineLarge`/... , `TitleLarge`/..., `BodyLarge`/..., `LabelLarge`/... Each style bundles FontSize, FontWeight, LineHeight, LetterSpacing. Controls reference via `Style="{StaticResource LabelLarge}"`.
- **`Tokens/Motion.axaml`** — Easing resources (Emphasized, Standard, Linear cubic bezier curves) and duration constants (200ms, 300ms) as defined by the MD3 motion spec.
- **`Tokens/_index.axaml`** — Merges all above.

### Module 4: Control Templates — MVP (Button, Card, TextBox, CheckBox, ToggleSwitch)

Each control follows this pattern:

1. A `ControlTheme` with `x:Key="{x:Type Button}"` (or equivalent) defining:
   - A `ControlTemplate` whose root element embeds a **State Layer** `Border` (opacity 0 by default)
   - Color references via `{MdSysColor Primary}`, `{MdSysColor OnPrimary}`, etc.
   - Shape references via `{StaticResource ShapeCornerFull}`
   - Typography via `Style="{StaticResource LabelLarge}"`
   - Elevation via `{MdElevation 1}` / `{MdSurface 1}`

2. **Variant themes** (e.g., `FilledButton`, `TonalButton`) as separate `ControlTheme` entries with `BasedOn="{StaticResource {x:Type Button}}"` — only overriding color and shape keys.

3. **State Layer** embedded as:
   ```xml
   <Border x:Name="StateLayer"
           Background="{Binding $parent.Foreground}"
           Opacity="0" IsHitTestVisible="False" />
   ```
   Controlled via pseudo-class selectors in the ControlTheme:
   - `:pointerover → /template/ StateLayer#StateLayer.Opacity = 0.08`
   - `:focus → 0.12`
   - `:pressed → 0.12`

### Module 5: MaterialYouTheme Entry Point

`MaterialYouTheme.axaml` is a `<Styles>` root that merges:
- `Controls/_index.axaml`
- `Tokens/_index.axaml`
- `Icons/_index.axaml` (via MaterialSymbols)
- `Locale/{en-US,zh-CN}.axaml`

It does NOT set up a `MaterialColor.Scheme`. That responsibility is delegated to the consumer's code-behind.

### Module 6: Demo Application

A `MaterialYou.Avalonia.Demo.Desktop` project that:
- Sets up `MaterialColor.Scheme` in `App.xaml.cs` (with system accent color detection + fallback)
- Provides a control gallery page showing all MVP controls in their various states
- Includes a seed-color picker to demonstrate runtime scheme switching

### Module 7: MaterialSymbols Project (Deferred)

The icon project structure is scaffolded (`src/MaterialYou.Avalonia.MaterialSymbols/`) but actual icon content generation is deferred. For MVP, icons are limited to a small hand-crafted set or omitted until the automation script is built.

## Deep Module Analysis

| Module | Depth | Testable In Isolation? | Rationale |
|--------|-------|----------------------|-----------|
| **DynamicColor** | Deep ✅ | Yes | Encapsulates HCT/scheme/elevation math behind `{MdSysColor Primary}` + `MaterialColor.SetScheme()`. One-line setup, simple consumption, complex internals. |
| **Control Templates** | Shallow | No (visual) | Inevitably detailed XAML with template parts, state layers, and pseudo-class selectors. Each control is explicit, not abstracted. |
| **Design Tokens** | Shallow | N/A | Pure data (CornerRadius, Style, Easing). No logic to test. |
| **Build Infra** | Shallow | N/A | Mechanical rename/delete. |
| **Demo App** | Shallow | No | Visual smoke test. |

## Testing Decisions

**What makes a good test:** Only test external behavior, not implementation details. For the DynamicColor module, test that given a specific seed color, the scheme produces expected colors for primary/surface/error roles. Do not test internal HCT math (that's the upstream `MaterialColorUtilities` package's responsibility).

**Modules to test:**
- **DynamicColor** — unit tests verifying:
  - Scheme generation from seed color produces correct Light and Dark scheme values (spot-check against known MD3 reference values)
  - Theme variant switch (Light→Dark) triggers correct re-resolution
  - Elevation returns different BoxShadow for each level 0–5
  - Surface + tint pre-blend produces expected output for known inputs

**Modules NOT to test (MVP):**
- Control templates (visual — covered by demo app manual verification)
- Design tokens (pure data)
- Build infrastructure (mechanical)

**Prior art:** No existing tests in this codebase (Semi.Avalonia has no test project). The DynamicColor tests follow standard .NET xUnit/nUnit patterns for color math verification.

## Out of Scope

- HighContrast theme variant (deferred until post-MVP)
- Non-MVP controls (List, Menu, Dialog, Snackbar, TabStrip, TopAppBar, NavigationBar, Chip, Slider, ProgressBar, Tooltip, etc.)
- DataGrid/TreeDataGrid/ColorPicker companion packages
- Web, Android, DRM demo platforms
- MaterialSymbols icon automation script and full icon set
- Non-Rounded icon styles (Fill, Outlined, Sharp)
- Runtime seed color extraction from wallpapers (Quantize/Score from MaterialColorUtilities)
- Custom/Extended color tokens (beyond the 5 MD3 roles + Error)

## Further Notes

- The `MaterialColorUtilities` NuGet package is Apache-2.0 licensed. Attribution must be preserved in the DynamicColor project.
- The Avalonia integration code adapted from `Shirasagi0012/MaterialColorUtilities` is also Apache-2.0. Attribution file required.
- The original Semi.Avalonia code being retained (build infrastructure, project structure) is MIT licensed.
- Semi's `ApplicationExtension.RegisterFollowSystemTheme()` pattern (Windows system theme detection via `PlatformSettings.ColorValuesChanged`) should be adapted for the demo app, not included in the library itself.
- After MVP validation, the next priority is completing the 20-control core set and the MaterialSymbols automation pipeline.
