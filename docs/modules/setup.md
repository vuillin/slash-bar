# setup — Profiles

Launch a set of applications and place their windows on screen.

Profiles are **not** hard-coded. Edit:

`%LocalAppData%\SlashBar\setup-profiles.json`

If the file is missing, SlashBar creates an empty `[]`. Copy the example from [`docs/examples/setup-profiles.example.json`](../examples/setup-profiles.example.json) and adapt paths to your machine.

`fileName` supports environment variables (`%LOCALAPPDATA%`, `%ProgramFiles%`, etc.).

## Syntax

```
setup <profile>
```

## Available layouts

| Layout | Description |
|--------|-------------|
| `Default` | No special placement |
| `Maximize` | Maximized window |
| `Minimized` | Minimized window |
| `LeftHalf` | Left half (right monitor) |
| `RightHalf` | Right half (right monitor) |
| `RightMonitor` | Full screen on the right monitor |

## JSON shape

```json
[
  {
    "name": "dev",
    "description": "My work setup",
    "steps": [
      {
        "fileName": "%LOCALAPPDATA%\\Programs\\Microsoft VS Code\\Code.exe",
        "layout": "Maximize",
        "windowProcessName": "Code"
      }
    ]
  }
]
```

- `fileName`: executable path (env vars expanded)
- `arguments`: optional CLI arguments
- `layout`: window placement (see table above)
- `windowProcessName`: process name used to find the window (e.g. `firefox`, `Cursor`)

Edits to the JSON are picked up on the next `setup` command (no restart required).

## Example

```
setup dev
```
