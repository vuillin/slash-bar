using System.Diagnostics;

namespace SlashBar.Modules;

/// <summary>
/// Ouvre la doc officielle (accueil ou recherche si dispo)
/// </summary>
public sealed class DocModule : IModule {

    // SearchUrl null = pas de recherche native → toujours l'accueil
    private sealed record DocSource(string Id, string Label, string HomeUrl, string? SearchUrl);

    // {0} = requête déjà encodée
    private static readonly DocSource[] All = [

        // langages
        new("php", "PHP",
            "https://www.php.net/manual/en/",
            "https://www.php.net/search.php?pattern={0}"),
        new("py", "Python 3",
            "https://docs.python.org/3/",
            "https://docs.python.org/3/search.html?q={0}"),
        new("js", "JavaScript (MDN)",
            "https://developer.mozilla.org/en-US/docs/Web/JavaScript",
            "https://developer.mozilla.org/en-US/search?q={0}"),
        new("ts", "TypeScript",
            "https://www.typescriptlang.org/docs/",
            null),
        new("java", "Java (Oracle)",
            "https://docs.oracle.com/en/java/",
            "https://docs.oracle.com/en/java/javase/21/docs/api/search.html?q={0}"),
        new("cs", "C# / .NET",
            "https://learn.microsoft.com/en-us/dotnet/csharp/",
            "https://learn.microsoft.com/en-us/search/?terms={0}"),
        new("dotnet", ".NET",
            "https://learn.microsoft.com/en-us/dotnet/",
            "https://learn.microsoft.com/en-us/search/?terms={0}&scope=.NET"),
        new("cpp", "C++ (cppreference)",
            "https://en.cppreference.com/w/cpp",
            "https://en.cppreference.com/index.php?search={0}"),
        new("c", "C (cppreference)",
            "https://en.cppreference.com/w/c",
            "https://en.cppreference.com/index.php?search={0}"),
        new("rust", "Rust std",
            "https://doc.rust-lang.org/std/",
            "https://doc.rust-lang.org/std/?search={0}"),
        new("go", "Go packages",
            "https://pkg.go.dev/std",
            "https://pkg.go.dev/search?q={0}"),
        new("ruby", "Ruby",
            "https://docs.ruby-lang.org/en/",
            null),
        new("kt", "Kotlin",
            "https://kotlinlang.org/docs/home.html",
            "https://kotlinlang.org/docs/home.html?q={0}&s=full"),
        new("swift", "Swift / Apple",
            "https://developer.apple.com/documentation/swift",
            "https://developer.apple.com/search/?q={0}"),
        new("lua", "Lua",
            "https://www.lua.org/manual/5.4/",
            null),

        // web / front
        new("mdn", "MDN Web Docs",
            "https://developer.mozilla.org/",
            "https://developer.mozilla.org/en-US/search?q={0}"),
        new("html", "HTML (MDN)",
            "https://developer.mozilla.org/en-US/docs/Web/HTML",
            "https://developer.mozilla.org/en-US/search?q={0}"),
        new("css", "CSS (MDN)",
            "https://developer.mozilla.org/en-US/docs/Web/CSS",
            "https://developer.mozilla.org/en-US/search?q={0}"),
        new("node", "Node.js",
            "https://nodejs.org/api/",
            null),
        new("react", "React",
            "https://react.dev/",
            null),
        new("vue", "Vue.js",
            "https://vuejs.org/",
            null),
        new("angular", "Angular",
            "https://angular.dev/overview",
            null),
        new("laravel", "Laravel",
            "https://laravel.com/docs",
            null),
        new("symfony", "Symfony",
            "https://symfony.com/doc/current/index.html",
            "https://symfony.com/doc?q={0}"),
        new("flask", "Flask",
            "https://flask.palletsprojects.com/",
            "https://flask.palletsprojects.com/en/stable/search/?q={0}"),

        // data / infra
        new("mysql", "MySQL",
            "https://dev.mysql.com/doc/",
            "https://dev.mysql.com/doc/search/?q={0}"),
        new("pg", "PostgreSQL",
            "https://www.postgresql.org/docs/current/",
            "https://www.postgresql.org/search/?q={0}"),
        new("mongo", "MongoDB",
            "https://www.mongodb.com/docs/",
            "https://www.mongodb.com/docs/search/?q={0}"),
        new("redis", "Redis",
            "https://redis.io/docs/",
            null),
        new("sqlite", "SQLite",
            "https://www.sqlite.org/docs.html",
            "https://www.sqlite.org/search?q={0}"),
        new("docker", "Docker",
            "https://docs.docker.com/",
            null),
        new("git", "Git",
            "https://git-scm.com/doc",
            "https://git-scm.com/search/results?search={0}"),
        new("linux", "Linux man (die.net)",
            "https://linux.die.net/man/",
            "https://www.die.net/search/#gsc.q={0}"),
        new("bash", "Bash / GNU",
            "https://www.gnu.org/software/bash/manual/",
            "https://www.gnu.org/cgi-bin/estseek.cgi?phrase={0}"),

        // outils / packages
        new("npm", "npm packages",
            "https://www.npmjs.com/",
            "https://www.npmjs.com/search?q={0}"),
        new("pypi", "PyPI",
            "https://pypi.org/",
            "https://pypi.org/search/?q={0}"),
        new("maven", "Maven Central",
            "https://search.maven.org/",
            "https://search.maven.org/search?q={0}"),
    ];

    private static readonly Dictionary<string, DocSource> ById =
        All.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);

    private static readonly ArgCompletion[] Flags = All
        .OrderBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
        .Select(s => new ArgCompletion(s.Id, s.Label))
        .ToArray();

    public string Prefix => "doc";
    public string Name => "Documentation";
    public string Description => "Ouvre la documentation";


    public void Execute(string argument) {

        argument = argument.Trim();
        if (argument.Length == 0)
            return;

        var space = argument.IndexOf(' ');
        var lang = space < 0 ? argument : argument[..space];
        var query = space < 0 ? "" : argument[(space + 1)..].Trim();

        if (!ById.TryGetValue(lang, out var source))
            return;

        // recherche seulement si requête + URL de search native
        var url = query.Length > 0 && source.SearchUrl != null
            ? string.Format(source.SearchUrl, Uri.EscapeDataString(query))
            : source.HomeUrl;

        OpenTab(url);
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {

        ModuleArgs.SplitCurrentToken(argument, out var before, out var token);

        if (before.Length == 0)
            return ModuleArgs.SuggestFlags(token, Flags);

        return Array.Empty<ArgCompletion>();
    }


    private static void OpenTab(string url) {

        Process.Start(new ProcessStartInfo {
            FileName = "firefox",
            Arguments = $"-new-tab \"{url}\"",
            UseShellExecute = true
        });
    }
}
