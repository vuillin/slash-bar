# calendar — Calendar

Day timeline, month grid, and local events with optional recurrence.

## Syntax

```
calendar          → toggle the panel
```

## Panel

| Area | Behavior |
|------|----------|
| Left | Selected day name + date, hour timeline, red “now” marker when viewing today |
| Center | Month navigation and day grid |
| Right form | Event title, Repeat menu, Add Event |
| Drawer | **View events** expands a side list with filters and delete |

### Month grid

- Selected day: red circle
- Today (if not selected): light gray circle
- Days with at least one occurring event: purple day number (lighter purple outside the current month)

### Events

Create with a title, optional repeat, on the selected date.

| Repeat | Meaning |
|--------|---------|
| Never | Only the start date |
| Every Day | Every day from the start date |
| Every Week | Same weekday |
| Every 2 Weeks | Every 14 days |
| Every Month | Same day of month (clamps to month end when needed) |
| Every Year | Same month/day each year |

Persisted at `%LocalAppData%/SlashBar/calendar-events.json`.

### Events drawer filters

| Filter | Shows |
|--------|--------|
| All | Every stored event |
| Past | One-shot (`Never`) events before today |
| Recurring | Events whose repeat is not `Never` |

Trash removes the event from storage (all future occurrences go away with it).

## Examples

```
calendar
```
