# SlashBar

A Windows shortcut bar. Type a command and run it — with icon rails, side panels, and history.

## Install (users)

1. Download the zip from the [latest Release](https://github.com/vuillin/slash-bar/releases/latest) (`SlashBar-v*-win-x64.zip`).
2. Extract the folder.
3. Run `SlashBar.exe`.

No .NET install required (self-contained build). Firefox must be on your PATH for the `f` module.

## Interface

Press `Ctrl+Space` to show the bar at the bottom of the screen.

**Bar** — type a command (e.g. `calc 2+2`, `f docs`); suggestions appear below. `Tab` completes, `Enter` runs. `↑` / `↓` cycle suggestions / parameters first, then **command history**.

**Rails** — icons on each side of the bar open common tools:
- left: Memo
- right: Color Picker, Clipboard

**Panels** — Memo, Color, and Clip open as docked side panels (only one at a time). You can collapse, detach, or close them. They also open via command (`memo`, `color`, `clip`).

**History** — clipboard and picked colors keep a history in their panels; bar commands can be replayed with the arrow keys.

## Shortcuts

| Key | Action |
|-----|--------|
| `Ctrl+Space` | Open / close the bar |
| `Ctrl+Shift+Q` | Quit |
| `Esc` | Close |
| `Tab` | Complete the selected command or parameter |
| `↑` / `↓` | Cycle suggestions / parameters, then command history |
| `Enter` | Run |

## Modules

See [docs/](docs/README.md).

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## Development

Prerequisite: [.NET SDK](https://dotnet.microsoft.com/download) (Windows), compatible with `net10.0-windows`.

```powershell
cd SlashBar
.\run.ps1
```

Or directly:

```powershell
cd SlashBar
dotnet run
```

To build a local Release zip (same layout as CI):

```powershell
cd SlashBar
.\publish.ps1
```

The zip is written to `publish/SlashBar-v*-win-x64.zip`. GitHub Releases are created automatically when you push a `v*` tag (e.g. `v1.0.0`).

## License

[MIT](LICENSE) — Copyright (c) 2026 Thomas Vuillin.
