# pin — Pin

Keep one or more windows always on top.

## Hotkey

Default: `Ctrl+Shift+A` — toggle pin on the foreground window (works without opening the bar). Remap under Settings → Hotkeys.

## Syntax

```
pin        → status (how many windows are pinned)
pin off    → unpin all
```

## Behavior

| Action | Effect |
|--------|--------|
| Pin hotkey on an unpinned window | Pin it (always on top) + accent border + toast |
| Pin hotkey on a pinned window | Unpin it (border removed) + toast |
| Several windows | Each can be pinned independently |
| Drag / resize a pinned window | Border hides while moving, then snaps back |
| Minimize a pinned window | Border hides until restored |
| Quit SlashBar | All pins are cleared |

Toasts (pin / unpin / `pin` status / `pin off`) stay visible ~3.2s.

SlashBar’s own windows are ignored (so the bar / toasts are never pinned).
