# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/vuillin/slash-bar/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/vuillin/slash-bar/releases/tag/v1.0.0
