# settings — Settings

App preferences panel.

## Syntax

```
settings          → toggle the panel
```

## Panel

Also opens from the gear icon to the right of `esc` on the bar, or from the tray menu (Settings).

Preferences persist in `%LocalAppData%\SlashBar\settings.json` and apply live where relevant.

| Section | Options |
|---------|---------|
| **Weather** | Default city (empty → Windows location, then IP), unit °C / °F, show sunrise/sunset in the hourly strip. Used by the Weather panel. |
| **Hotkeys** | Open bar, Quit, Pin window — click a shortcut, press a new combo (`Ctrl` / `Shift` / `Alt` + key), or reset to default. Changes re-register immediately. |
| **Display** | Theme Light / Dark — bar, panels, and toasts. |

Defaults: Open bar `Ctrl+Space`, Quit `Ctrl+Shift+Q`, Pin `Ctrl+Shift+A`.

## Examples

```
settings
```
