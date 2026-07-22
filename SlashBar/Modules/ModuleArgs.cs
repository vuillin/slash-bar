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
    /// Suggestions pour le 1er mot
    /// </summary>
    public static IReadOnlyList<ArgCompletion> SuggestFlags(

        string argument,
        IReadOnlyList<ArgCompletion> flags) {

        if (argument.Contains(' '))
            return Array.Empty<ArgCompletion>();

        return flags
            .Where(f => f.Value.StartsWith(argument, StringComparison.OrdinalIgnoreCase)
                        && !f.Value.Equals(argument, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}