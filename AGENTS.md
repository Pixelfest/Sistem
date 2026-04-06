# Agents Guide – Sistem

## Project overview

Sistem is a stereogram generator written in C#/.NET 10. It produces single-image stereograms (SIS) from depth maps and optional pattern images. The solution ships a core library, a CLI tool, and a WPF desktop application that supports multi-layer stereogram composition.

## Solution structure

| Project | Type | TFM | Description |
|---|---|---|---|
| `Sistem.Core` | Class library | `net10.0` | Core stereogram generation engine. Platform-independent. |
| `Sistem.CommandLine` | Console (exe: `sis`) | `net10.0` | CLI wrapper around `Sistem.Core`. |
| `OpenStereogramCreator` | WPF application | `net10.0-windows7.0` | Multi-layer stereogram editor with a layer-based UI. |
| `TestApp` | Console | `net10.0` | Manual test harness (not a test framework project). |

Two solution files exist:

- `Sistem.sln` – all four projects (Windows, includes WPF).
- `Sistem.Standard.sln` – `Sistem.Core` + `Sistem.CommandLine` only (cross-platform).

## Build

```
dotnet build Sistem.sln            # Windows (includes WPF)
dotnet build Sistem.Standard.sln   # Cross-platform
```

Platform configuration: `Sistem.Core` and `Sistem.CommandLine` target `x64`. `OpenStereogramCreator` supports `AnyCPU` and `x64`. There is no `global.json`.

## Key dependencies

- **SixLabors.ImageSharp** (`3.1.12`) – image loading, manipulation, and pixel-level access. Used across all projects.
- **SixLabors.ImageSharp.Drawing** (`2.1.7`) – used by `OpenStereogramCreator` for font/drawing operations.
- **McMaster.Extensions.CommandLineUtils** – CLI argument parsing in `Sistem.CommandLine`.

## Code style & conventions

### Formatting (`.editorconfig`)

- Indentation: **tabs**, tab width 4.
- Line endings: CRLF.
- Final newline: yes, trailing whitespace trimmed.
- `var` is preferred everywhere (`csharp_style_var_*` = `true:warning`).

### C# patterns in use

- `LangVersion` is `default` (C# 14 on .NET 10).
- File-scoped namespaces in `Sistem.Core`; block-scoped namespaces in `OpenStereogramCreator` and `Sistem.CommandLine`.
- Records for DTOs/options (`StereogramOptions` is a `record`).
- `INotifyPropertyChanged` with `[CallerMemberName]` for WPF view models.
- Strategy pattern for algorithms (`IStereogramAlgorithm` → `RandomDotAlgorithm`, `PatternAlgorithm`).
- `AllowUnsafeBlocks` is enabled in `OpenStereogramCreator`.

### Naming

- Layer view model classes are named by their layer type: `RandomDotStereogramLayer`, `PatternStereogramLayer`, `FullImageStereogramLayer`, `ImageLayer`, `RepeaterLayer`, `ReversePatternLayer`.
- DTO classes mirror view models with a `Dto` suffix in `OpenStereogramCreator\Dtos\`.
- User controls for layer property panels live in `OpenStereogramCreator\UserControls\`.

## Architecture

### Sistem.Core

The core engine is under `Sistem.Core\Generation\`:

| File | Role |
|---|---|
| `IStereogramGenerator` | Public entry point interface. |
| `StereogramGenerator` | Default implementation. Validates, selects algorithm, processes lines, builds result. |
| `StereogramOptions` | Immutable record with all generation parameters (depth map, pattern, separations, oversampling, etc.). |
| `StereogramResult` | Output: generated image, errors, warnings, and `Success` flag. |
| `StereogramValidator` | Validates options; returns errors and warnings. |
| `IStereogramAlgorithm` | Internal strategy for line rendering. |
| `RandomDotAlgorithm` | Random-dot stereogram rendering. |
| `PatternAlgorithm` | Pattern-based stereogram rendering. |
| `OversamplingContext` | Pre-computed dimensions, shared state, and oversampled images. |
| `RandomDotPatternProvider` | Generates noise patterns for random-dot mode. |
| `ImageIO` | Image load/save utilities. |

`ImageProcessing.cs` (root of `Sistem.Core`) provides auxiliary operations like shadow generation.

Algorithm selection: random-dot is used when no pattern is provided and oversampling is 1; otherwise the pattern algorithm is used.

Line processing supports `Parallel.For` for multi-threaded rendering.

### OpenStereogramCreator

WPF app with a layer-based architecture:

- **ViewModels** (`ViewModels\`): `LayerBase` → `StereogramLayer` → concrete layers. Each layer type has `Render()`, `Export<T>()`, and `Import<T>()` methods.
- **DTOs** (`Dtos\`): Serializable data transfer objects for saving/loading layer configurations.
- **UserControls** (`UserControls\`): XAML property panels bound to view models.
- **MainWindow** is split across partial classes: `MainWindow.xaml.cs`, `MainWindow.Drawing.cs`, `MainWindow.Layers.cs`, `MainWindow.MenuEventHandlers.cs`, `MainWindow.Zooming.cs`.
- Interfaces: `IHaveADepthImage`, `IHaveAPattern` for layer capability contracts.

### Sistem.CommandLine

Single-file CLI (`Program.cs`) using attribute-based command definition. Entry point: `Sistem.CommandLine.Program`. Output binary: `sis.dll` / `sis.exe`.

## Important pixel types

The codebase uses specific ImageSharp pixel types throughout:

- `Image<Rgb48>` – depth maps (16-bit per channel for depth precision).
- `Image<Rgba32>` – patterns, rendered output, and layer cached images.

## Testing

There is no automated test framework project in the solution. `TestApp` is a manual console harness. There are no unit tests to run.

## Working with this codebase

- When modifying stereogram generation logic, focus on `Sistem.Core\Generation\`.
- When adding or changing layer types, update the view model, DTO, user control, and layer management code in `OpenStereogramCreator`.
- `StereogramOptions` is a record; extend it with `init`-only properties and update `StereogramValidator` for any new constraints.
- Keep `Sistem.Core` platform-independent; no WPF or Windows dependencies.
- The `IStereogramAlgorithm` interface is `internal`; external consumers use `IStereogramGenerator`.
