# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- `f` (Firefox search) renamed to `web`: opens / searches in the default browser; `doc` uses the same helper (no Firefox PATH dependency)

## [1.4.0] - 2026-09-09

### Added

- `pin` module: keep windows always on top; `Ctrl+Shift+A` toggles the foreground window; multi-pin; semi-transparent accent border (hides while dragging/resizing); `pin` / `pin off` from the bar; pin toasts ~3.2s
- Shared `Modules/Native` layer for Win32 / DWM (single P/Invoke surface + helpers for windows, hotkeys, clipboard listeners, execution state, DPI screen placement)
- Unified `ToastHost`: one top-right window with sticky (Awake) and ephemeral (success / error) slots so they stack instead of overlapping

### Changed

- README shortcuts list includes `Ctrl+Shift+A` (pin)
- Weather, geolocation, and `ip` run async end-to-end (no `.GetResult()` on the UI thread); `IpModule.Execute` stays sync-safe (`ip local` only) so a sync `TryExecute` cannot re-block on HTTP
- `setup` launches in the background and returns immediately with a toast; window wait/place uses `Task.Delay` instead of `Thread.Sleep`
- `setup` process start uses `UseShellExecute = true` so `.lnk` and typical GUI targets open reliably
- Module commands that copy to the clipboard call `IgnoreNext()` first so SlashBar does not pollute clipboard history
- Memo / clipboard / color JSON stores debounce disk writes (~400 ms) to avoid sync I/O bursts; pending saves flush on app exit
- Side panel headers share `SidePanelHeader` (traffic lights + drag); orange control documented as minimize-to-shelf
- `AppToast` / `AwakeToast` are thin facades over `ToastHost` (in-panel Copied toasts unchanged)
- `awake` re-asserts `SetThreadExecutionState` every second while active (heartbeat), including when there is no duration

### Fixed

- Pin: closing a pinned window fully tears down session state and WinEvent hooks (no ghost “pinned” until the next prune)
- Toast host: dismissing an ephemeral toast while Awake is sticky no longer makes the Awake card jump
- `setup`: background failures show an error toast instead of failing silently
- `ip local`: clear error when no LAN address is available (no fake `127.0.0.1` success)
- Hotkey registration: clearer message when one or more shortcuts fail
- Firefox launch (`f` / `doc`): clearer error when Firefox is missing

## [1.3.0] - 2026-09-07

### Added

- `awake` module: keep system (and optionally display) awake; optional duration (`30m`, `2h`, bare minutes); max 24h; sticky toast with countdown; click toast to turn off
- Side panel minimize: yellow traffic-light sends a panel to a bottom-left icon shelf; click the icon to restore it
- Color panel: eyedropper toggle in the header (on by default)

### Changed

- Side panels can stay open together; opening one no longer closes the others
- Yellow traffic-light minimizes to the shelf instead of re-docking a detached panel
- README and module docs updated for multi-panel, shelf, and `awake`

## [1.2.0] - 2026-09-04

### Added

- Clipboard history: content-type icons (color, code, file path, URL, email, long text, other)
- Refreshed Color, Clipboard, and Memo panel layouts and shortcut icons

### Changed

- Side panels no longer collapse to a dock tab; drag to detach and reset to dock remain
- Side panel chrome simplified (removed collapse / chevron controls)
- README updated for panel behavior

### Removed

- Panel collapse / expand (dock tab) on side panels

## [1.1.0] - 2026-09-01

### Added

- `weather` module: top-docked panel with current conditions, daily high/low, and the next 6 hours (Open-Meteo; no API key)
- Weather location, in order: city in `%LocalAppData%\SlashBar\weather.json`, then Windows Geolocation, then public IP (`ipwho.is`); geocoded coordinates are saved back into `weather.json`
- Weather forecast cache in `%LocalAppData%\SlashBar\weather-cache.json` (15-minute TTL); stale snapshot shown immediately on open, background refresh when stale, auto-refresh while the panel stays open
- Sunrise and sunset slots in the hourly strip when they fall within the window
- Custom weather icons (`Assets/Icons/Weather/`)
- Weather shortcut on the left icon rail
- Per-module gradient tile backgrounds on pinned shortcut tiles
- `gen lorem`: copy a Lorem Ipsum paragraph
- `gen up` / `gen low`: copy text as uppercase / lowercase (`fr-FR`)

### Changed

- Left shortcut rail: Memo, Weather (was Memo only in 1.0.0)
- README and module docs updated for Weather and `gen` subcommands

## [1.0.0] - 2026-07-31

First public release.

### Added

- Windows shortcut bar with command input, suggestions, and command history
- Side icon rails and docked panels for Memo, Color Picker, and Clipboard
- Modules: `f`, `gen`, `doc`, `setup`, `ip`, `color`, `clip`, `memo`, `calc`
- Self-contained `win-x64` Release zip via GitHub Actions on `v*` tags
- Local `publish.ps1` for the same Release layout
- Setup profiles loaded from `%LocalAppData%\SlashBar\setup-profiles.json` (example included)
- MIT license
- English README and module documentation

### Changed

- Dev launcher (`run.ps1`) uses `dotnet` from PATH / `DOTNET_ROOT` instead of a machine-specific SDK path
- Full English UI, module messages, and in-code comments

[Unreleased]: https://github.com/vuillin/slash-bar/compare/v1.4.0...HEAD
[1.4.0]: https://github.com/vuillin/slash-bar/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/vuillin/slash-bar/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/vuillin/slash-bar/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/vuillin/slash-bar/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/vuillin/slash-bar/releases/tag/v1.0.0
