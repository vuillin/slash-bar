# f — Recherche Firefox

Recherche web ou ouverture d'URL dans Firefox.

## Syntaxe

```
f
f <requête | url>
f private <requête | url>
```

## Paramètres

| Paramètre | Description |
|-----------|-------------|
| `private` | Navigation privée |

## Exemples

```
f
f chatgpt
f github.com
f private reddit.com
f private
```

- `f` seul : lance Firefox
- Texte libre : recherche via le moteur par défaut de Firefox
- Entrée de type URL (`github.com`, `https://…`) : ouverture de la page
- `f private` seul : ouverture d'une fenêtre privée
