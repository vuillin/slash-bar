# clip — Clipboard

Open the clipboard history.

## Syntax

```
clip
```

## Behavior

| Action | Effect |
|--------|--------|
| Click an entry | Copy the text again and show “Copied” |
| Trash icon | Delete the entry |
| Clear all | Empty the history |

Each entry shows an icon based on content type (color, code, file path, URL, email, long text, or other).

History is persisted at `%LocalAppData%/SlashBar/clipboard-history.json`.

## Examples

```
clip
```
