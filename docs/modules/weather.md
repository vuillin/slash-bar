# weather — Weather

Open the weather panel (current conditions).

Location, in order:

1. Default city from Settings (`%LocalAppData%\SlashBar\settings.json`) if set
2. Otherwise Windows location (needs Location enabled in Windows settings, including desktop apps)
3. Otherwise the public IP (`ipwho.is`)

Unit (°C / °F) and sunrise/sunset in the hourly strip also come from Settings.

Forecast from [Open-Meteo](https://open-meteo.com/). No API key.

After the first successful lookup for a saved city, coordinates are cached in `%LocalAppData%\SlashBar\weather.json`:

```json
{
  "geocode": {
    "cityQuery": "Lyon",
    "latitude": 45.758,
    "longitude": 4.835,
    "label": "Lyon, France"
  }
}
```

Forecast data is cached separately in `%LocalAppData%\SlashBar\weather-cache.json` (15-minute TTL). On open, the last snapshot is shown immediately when available; a background refresh runs if the cache is stale. While the panel stays open, it refreshes automatically every 15 minutes, and again when Weather settings change.

Leave the default city empty in Settings to use Windows location, then IP. Open-Meteo picks the most populous match for a saved city. A VPN only matters when no city is set and Windows location is unavailable.

## Syntax

```
weather              → toggle the panel
```

## Panel

| Field | Source |
|-------|--------|
| Place | Settings city, Windows location, or city / country from the public IP (header) |
| Temperature | Current temperature (°C or °F from Settings) |
| Condition | WMO weather code + icon (Clear, Rain, …) |
| High / low | Daily max / min |
| Next hours | Next 6 hours: time, icon, temperature (sunrise/sunset slots if enabled in Settings) |

## Examples

```
weather
```
