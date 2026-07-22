# gen — Générateur

Génère une valeur et la copie dans le presse-papiers.

## Syntaxe

```
gen b64 encode <texte>
gen b64 decode <base64>
```

## Types

| Type | Sous-commande | Description |
|------|---------------|-------------|
| `b64` | `encode` | Encodage Base64 (UTF-8) |
| `b64` | `decode` | Décodage Base64 |

## Exemples

```
gen b64 encode hello
gen b64 decode aGVsbG8=
```
