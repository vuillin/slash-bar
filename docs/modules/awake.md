# awake — Keep awake

Prevent sleep / display off without changing power settings.

## Syntax

```
awake              → toggle (system + display)
awake system       → toggle (system only; display may sleep)
awake 30m          → on for 30 minutes
awake 25           → on for 25 minutes
awake 2h           → on for 2 hours
awake system 30m   → system only, 30 minutes
awake 2h system    → same (order free)
```

## Modes

| Mode | Effect |
|------|--------|
| _(default)_ | System + display stay awake |
| `system` | System stays awake; display may turn off |

## Duration

| Token | Meaning |
|-------|---------|
| `25` | 25 minutes |
| `30m` | 30 minutes |
| `2h` | 2 hours |

Bare numbers are minutes. `system` and duration can appear in any order; only one duration is allowed.
Maximum duration: **24h**.

## Behavior

| Input | While off | While on |
|-------|-----------|----------|
| No duration (`awake` / `awake system`) | Enable (unlimited) | Toggle off |
| With duration | Enable for that duration | Restart with new mode / duration |

When the timer ends, awake turns off automatically (sticky toast hides, brief “Awake off”).

Sticky toast shows mode and remaining time when a timer is set (e.g. `Awake · system (29m)`).
Click the toast to turn awake off.
