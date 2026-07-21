using System.Reflection;

namespace SlashBar.Modules;

public sealed class ModuleRegistry
{
    private readonly List<IModule> _modules;

    public ModuleRegistry()
    {
        _modules = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(IModule).IsAssignableFrom(t)
                        && t.GetConstructor(Type.EmptyTypes) != null)
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            // préfixes longs d'abord : "gh" avant "g"
            .OrderByDescending(m => m.Prefix.Length)
            .ThenBy(m => m.Prefix, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<IModule> Modules => _modules;

    public IReadOnlyList<IModule> Suggest(string input, int max = 5)
    {
        if (max <= 0)
            return Array.Empty<IModule>();

        input = input.Trim();
        if (input.Length == 0)
            return Array.Empty<IModule>();

        // "f chat" → on matche sur "f"
        var token = input.Split(' ', 2)[0];

        return _modules
            .Where(m => m.Prefix.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            .Take(max)
            .ToList();
    }

    public bool TryExecute(string input)
    {
        if (!TryResolve(input, out var module, out var argument))
            return false;

        module.Execute(argument);
        return true;
    }

    public bool TryResolve(string input, out IModule module, out string argument)
    {
        module = null!;
        argument = "";

        input = input.Trim();
        if (input.Length == 0)
            return false;

        foreach (var candidate in _modules)
        {
            var prefix = candidate.Prefix.Trim();
            if (prefix.Length == 0)
                continue;

            if (input.Equals(prefix, StringComparison.OrdinalIgnoreCase))
            {
                module = candidate;
                argument = "";
                return true;
            }

            var withSpace = prefix + " ";
            if (input.StartsWith(withSpace, StringComparison.OrdinalIgnoreCase))
            {
                module = candidate;
                argument = input[withSpace.Length..].Trim();
                return true;
            }
        }

        return false;
    }
}
