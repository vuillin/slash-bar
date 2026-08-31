# weather — Weather

Open the weather panel (current conditions).

Location comes from the public IP (`ipwho.is`). Forecast from [Open-Meteo](https://open-meteo.com/). No API key.

## Syntax

```
weather              → toggle the panel
```

## Panel

| Field | Source |
|-------|--------|
| Place | City / country from the public IP (header) |
| Temperature | Current temperature (°C) |
| Condition | WMO weather code + icon (Clear, Rain, …) |
| High / low | Daily max / min (°C) |
| Next hours | Next 6 hours: time, icon, temperature |

A VPN shows the weather for the VPN exit, not the physical location.

## Examples

```
weather
```
