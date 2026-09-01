# weather — Weather

Open the weather panel (current conditions).

Location, in order:

1. City in `%LocalAppData%\SlashBar\weather.json` if set
2. Otherwise Windows location (needs Location enabled in Windows settings, including desktop apps)
3. Otherwise the public IP (`ipwho.is`)

Forecast from [Open-Meteo](https://open-meteo.com/). No API key.

The file is created empty on first launch. Edit it by hand:

```json
{
  "city": "Lyon"
}
```

Leave `"city"` empty to use Windows location, then IP. Open-Meteo picks the most populous match for a saved city. A VPN only matters when no city is set and Windows location is unavailable. Edits to the JSON are picked up the next time the panel opens.

## Syntax

```
weather              → toggle the panel
```

## Panel

| Field | Source |
|-------|--------|
| Place | Saved city, Windows location, or city / country from the public IP (header) |
| Temperature | Current temperature (°C) |
| Condition | WMO weather code + icon (Clear, Rain, …) |
| High / low | Daily max / min (°C) |
| Next hours | Next 6 hours: time, icon, temperature |

## Examples

```
weather
```
