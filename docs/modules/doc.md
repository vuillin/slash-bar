# doc — Documentation

Ouvre la documentation officielle dans un nouvel onglet.

## Syntaxe

```
doc <source>              → page d'accueil
doc <source> <requête>    → recherche (si dispo), sinon accueil
```

Si la source n'a pas de recherche native, une requête ouvre quand même la page d'accueil.

## Sources

| Id | Documentation | Recherche |
|----|---------------|-----------|
| `php` | PHP | oui |
| `py` | Python 3 | oui |
| `js` | JavaScript (MDN) | oui |
| `ts` | TypeScript | non |
| `java` | Java (Oracle) | oui |
| `cs` | C# / .NET | oui |
| `dotnet` | .NET | oui |
| `cpp` | C++ (cppreference) | oui |
| `c` | C (cppreference) | oui |
| `rust` | Rust std | oui |
| `go` | Go packages | oui |
| `ruby` | Ruby | non |
| `kt` | Kotlin | oui |
| `swift` | Swift / Apple | oui |
| `lua` | Lua | non |
| `mdn` | MDN Web Docs | oui |
| `html` | HTML (MDN) | oui |
| `css` | CSS (MDN) | oui |
| `node` | Node.js | non |
| `react` | React | non |
| `vue` | Vue.js | non |
| `angular` | Angular | non |
| `laravel` | Laravel | non |
| `symfony` | Symfony | oui |
| `flask` | Flask | oui |
| `mysql` | MySQL | oui |
| `pg` | PostgreSQL | oui |
| `mongo` | MongoDB | oui |
| `redis` | Redis | non |
| `sqlite` | SQLite | oui |
| `docker` | Docker | non |
| `git` | Git | oui |
| `linux` | Linux man | oui |
| `bash` | Bash / GNU | oui |
| `npm` | npm packages | oui |
| `pypi` | PyPI | oui |
| `maven` | Maven Central | oui |

## Exemples

```
doc php
doc php array_map
doc ts
doc ts generics
doc py pathlib
doc react
```
