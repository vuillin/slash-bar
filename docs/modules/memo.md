# memo — Memo

Raccourcis texte (nom → valeur).

## Syntaxe

```
memo              → ouvre / ferme le panneau
memo <nom>        → copie la valeur
```

Noms en minuscules. Suggestions dynamiques (max 5).

## Panneau

| Action | Effet |
|--------|--------|
| Nom + Valeur + `+` | Ajoute |
| Stylo | Édite |
| Corbeille | Supprime |
| Clic carte | Copie la valeur |
| Recherche | Filtre nom / valeur |

Persistance : `%LocalAppData%/SlashBar/memos.json`.

## Exemples

```
memo
memo mail
memo tel
```
