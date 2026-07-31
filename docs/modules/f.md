# f — Firefox search

Web search or open a URL in Firefox.

## Syntax

```
f
f <query | url>
f private <query | url>
```

## Parameters

| Parameter | Description |
|-----------|-------------|
| `private` | Private browsing |

## Examples

```
f
f chatgpt
f github.com
f private reddit.com
f private
```

- `f` alone: launches Firefox
- Free text: search with Firefox’s default search engine
- URL-like input (`github.com`, `https://…`): opens the page
- `f private` alone: opens a private window
