# web — Web search

Web search or open a URL in the default browser.

## Syntax

```
web
web <query | url>
web private <query | url>
```

## Parameters

| Parameter | Description |
|-----------|-------------|
| `private` | Private / incognito browsing |

## Examples

```
web
web chatgpt
web github.com
web private reddit.com
web private
```

- `web` alone: launches the default browser
- Free text: Google search
- URL-like input (`github.com`, `https://…`): opens the page
- `web private` alone: opens a private / incognito window
- `web private <query>`: Google search in a private window
