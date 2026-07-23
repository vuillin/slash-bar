namespace SlashBar.Modules;

/// <summary>
/// Helpers communs pour parser les arguments des modules
/// </summary>
public static class ModuleArgs {


    /// <summary>
    /// Retire le flag s'il est en tête. "private foo" → true + "foo"
    /// </summary>
    public static bool ConsumeFlag(ref string argument, string flag) {

        if (argument.Equals(flag, StringComparison.OrdinalIgnoreCase)) {
            argument = "";
            return true;
        }

        var withSpace = flag + " ";

        if (argument.StartsWith(withSpace, StringComparison.OrdinalIgnoreCase)) {
            argument = argument[withSpace.Length..].Trim();
            return true;
        }

        return false;
    }


    /// <summary>
    /// découpe l'argument : tout sauf le dernier mot + le mot en cours
    /// "b64 encode" → ("b64 ", "encode")
    /// </summary>
    public static void SplitCurrentToken(string argument, out string before, out string token) {

        var lastSpace = argument.LastIndexOf(' ');
        if (lastSpace < 0) {
            before = "";
            token = argument;
            return;
        }

        before = argument[..(lastSpace + 1)];
        token = argument[(lastSpace + 1)..];
    }


    /// <summary>
    /// Suggestions pour le 1er mot
    /// </summary>
    public static IReadOnlyList<ArgCompletion> SuggestFlags(

        string argument,
        IReadOnlyList<ArgCompletion> flags,
        int max = 10) {

        if (argument.Contains(' '))
            return Array.Empty<ArgCompletion>();

        return flags
            .Where(f => f.Value.StartsWith(argument, StringComparison.OrdinalIgnoreCase)
                        && !f.Value.Equals(argument, StringComparison.OrdinalIgnoreCase))
            .Take(max)
            .ToList();
    }
}