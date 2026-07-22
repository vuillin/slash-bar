# gen — Générateur

Génère une valeur et la copie dans le presse-papiers.

## Syntaxe

```
gen b64 encode <texte>
gen b64 decode <base64>
gen hex encode <texte>
gen hex decode <hex>
gen date
gen time
gen timestamp
```

## Types

| Type | Sous-commande | Description |
|------|---------------|-------------|
| `b64` | `encode` | Encodage Base64 (UTF-8) |
| `b64` | `decode` | Décodage Base64 |
| `hex` | `encode` | Encodage hexadécimal (UTF-8) |
| `hex` | `decode` | Décodage hexadécimal |
| `date` | — | Date du jour (`jj-mm-aaaa`) |
| `time` | — | Heure actuelle (`hh:mm:ss`) |
| `timestamp` | — | Timestamp Unix UTC (secondes) |

## Exemples

```
gen b64 encode hello
gen b64 decode aGVsbG8=
gen hex encode hello
gen hex decode 68656c6c6f
gen date
gen time
gen timestamp
```
