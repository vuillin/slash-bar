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
            // "gh" avant "g"
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

        var token = input.Split(' ', 2)[0];

        return _modules
            .Where(m => m.Prefix.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            .Take(max)
            .ToList();
    }

    public IReadOnlyList<ArgCompletion> SuggestArgumentCompletions(string input)
    {
        if (!TryResolveArgument(input, out var module, out var argument))
            return Array.Empty<ArgCompletion>();

        return module.SuggestCompletions(argument);
    }

    public bool IsInArgumentMode(string input) =>
        TryResolveArgument(input, out _, out _);

    // "f pr" + "private" → "f private "
    public bool TryApplyCompletion(string input, string completion, out string newInput)
    {
        newInput = input;

        var raw = input.TrimStart();
        var leadingWs = input[..(input.Length - raw.Length)];

        foreach (var candidate in _modules)
        {
            var prefix = candidate.Prefix.Trim();
            if (prefix.Length == 0)
                continue;

            var withSpace = prefix + " ";
            if (!raw.StartsWith(withSpace, StringComparison.OrdinalIgnoreCase))
                continue;

            newInput = leadingWs + withSpace + completion + " ";
            return true;
        }

        return false;
    }

    // comme TryResolve mais sans trim l'argument
    private bool TryResolveArgument(string input, out IModule module, out string argument)
    {
        module = null!;
        argument = "";

        var raw = input.TrimStart();
        if (raw.Length == 0)
            return false;

        foreach (var candidate in _modules)
        {
            var prefix = candidate.Prefix.Trim();
            if (prefix.Length == 0)
                continue;

            var withSpace = prefix + " ";
            if (!raw.StartsWith(withSpace, StringComparison.OrdinalIgnoreCase))
                continue;

            module = candidate;
            argument = raw[withSpace.Length..];
            return true;
        }

        return false;
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
