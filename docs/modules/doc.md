# doc — Documentation

Open official documentation in a new tab.

## Syntax

```
doc <source>              → home page
doc <source> <query>      → search (if available), otherwise home
```

If the source has no native search, a query still opens the home page.

## Sources

| Id | Documentation | Search |
|----|---------------|--------|
| `php` | PHP | yes |
| `py` | Python 3 | yes |
| `js` | JavaScript (MDN) | yes |
| `ts` | TypeScript | no |
| `java` | Java (Oracle) | yes |
| `cs` | C# / .NET | yes |
| `dotnet` | .NET | yes |
| `cpp` | C++ (cppreference) | yes |
| `c` | C (cppreference) | yes |
| `rust` | Rust std | yes |
| `go` | Go packages | yes |
| `ruby` | Ruby | no |
| `kt` | Kotlin | yes |
| `swift` | Swift / Apple | yes |
| `lua` | Lua | no |
| `mdn` | MDN Web Docs | yes |
| `html` | HTML (MDN) | yes |
| `css` | CSS (MDN) | yes |
| `node` | Node.js | no |
| `react` | React | no |
| `vue` | Vue.js | no |
| `angular` | Angular | no |
| `laravel` | Laravel | no |
| `symfony` | Symfony | yes |
| `flask` | Flask | yes |
| `mysql` | MySQL | yes |
| `pg` | PostgreSQL | yes |
| `mongo` | MongoDB | yes |
| `redis` | Redis | no |
| `sqlite` | SQLite | yes |
| `docker` | Docker | no |
| `git` | Git | yes |
| `linux` | Linux man | yes |
| `bash` | Bash / GNU | yes |
| `npm` | npm packages | yes |
| `pypi` | PyPI | yes |
| `maven` | Maven Central | yes |

## Examples

```
doc php
doc php array_map
doc ts
doc ts generics
doc py pathlib
doc react
```
