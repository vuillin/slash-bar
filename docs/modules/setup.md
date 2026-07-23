# setup — Profils

Lance un ensemble d'applications et les place sur l'écran.

Les profils sont déclarés en dur dans `SlashBar/Modules/Setup/SetupProfiles.cs`.

## Syntaxe

```
setup <profil>
```

## Layouts disponibles

| Layout | Description |
|--------|-------------|
| `Default` | Aucun placement particulier |
| `Maximize` | Fenêtre maximisée |
| `Minimized` | Fenêtre réduite |
| `LeftHalf` | Moitié gauche (écran de droite) |
| `RightHalf` | Moitié droite (écran de droite) |
| `RightMonitor` | Plein écran sur l'écran de droite |

## Ajouter un profil

Dans `SetupProfiles.All` :

```csharp
new("nom", "Description", [
    new(@"C:\chemin\app.exe",
        Arguments: "--optionnel",
        Layout: WindowLayout.Maximize,
        WindowProcessName: "NomDuProcess"),
]),
```

- `FileName` : exécutable (ou raccourci `.lnk` avec `UseShellExecute` adapté)
- `Arguments` : arguments CLI optionnels
- `Layout` : placement de la fenêtre
- `WindowProcessName` : nom du process pour retrouver la fenêtre (ex. `firefox`, `Cursor`)

## Exemple

```
setup dev
```
