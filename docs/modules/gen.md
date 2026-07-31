# gen — Generator

Generate a value and copy it to the clipboard.

## Syntax

```
gen b64 encode <text>
gen b64 decode <base64>
gen hex encode <text>
gen hex decode <hex>
gen date
gen time
gen timestamp
gen lorem
```

## Types

| Type | Subcommand | Description |
|------|------------|-------------|
| `b64` | `encode` | Base64 encode (UTF-8) |
| `b64` | `decode` | Base64 decode |
| `hex` | `encode` | Hex encode (UTF-8) |
| `hex` | `decode` | Hex decode |
| `date` | — | Today’s date (`dd-mm-yyyy`) |
| `time` | — | Current time (`hh:mm:ss`) |
| `timestamp` | — | Unix UTC timestamp (seconds) |
| `lorem` | — | Lorem Ipsum paragraph |

## Examples

```
gen b64 encode hello
gen b64 decode aGVsbG8=
gen hex encode hello
gen hex decode 68656c6c6f
gen date
gen time
gen timestamp
gen lorem
```
