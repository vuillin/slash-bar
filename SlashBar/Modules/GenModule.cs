using System.Text;

namespace SlashBar.Modules;

public sealed class GenModule : IModule {

    private static readonly ArgCompletion[] Flags = [
        new("b64", "Base64 encode/decode"),
        new("hex", "Hex encode/decode"),
        new("date", "Today's date (dd-mm-yyyy)"),
        new("time", "Current time (hh:mm:ss)"),
        new("timestamp", "Unix timestamp (seconds)"),
        new("lorem", "Lorem Ipsum paragraph")
    ];

    private static readonly ArgCompletion[] B64Flags = [
        new("encode", "Encode to Base64"),
        new("decode", "Decode from Base64")
    ];

    private static readonly ArgCompletion[] HexFlags = [
        new("encode", "Encode to hexadecimal"),
        new("decode", "Decode from hexadecimal")
    ];

    public string Prefix => "gen";
    public string Name => "Generator";
    public string Description => "Generate a value (clipboard)";


    public ModuleResult Execute(string argument) {
        argument = argument.Trim();
        if (argument.Length == 0)
            return ModuleResult.Error("Argument required");

        var space = argument.IndexOf(' ');
        var cmd = space < 0 ? argument : argument[..space];
        var rest = space < 0 ? "" : argument[(space + 1)..].Trim();

        if (cmd.Equals("b64", StringComparison.OrdinalIgnoreCase))
            return RunB64(rest);

        if (cmd.Equals("hex", StringComparison.OrdinalIgnoreCase))
            return RunHex(rest);

        if (cmd.Equals("date", StringComparison.OrdinalIgnoreCase)) {
            var text = DateTime.Now.ToString("dd-MM-yyyy");
            ClipboardHelper.SetText(text);
            return ModuleResult.Copied(text);
        }

        if (cmd.Equals("time", StringComparison.OrdinalIgnoreCase)) {
            var text = DateTime.Now.ToString("HH:mm:ss");
            ClipboardHelper.SetText(text);
            return ModuleResult.Copied(text);
        }

        if (cmd.Equals("timestamp", StringComparison.OrdinalIgnoreCase)) {
            var text = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            ClipboardHelper.SetText(text);
            return ModuleResult.Copied(text);
        }

        if (cmd.Equals("lorem", StringComparison.OrdinalIgnoreCase)) {
            const string text =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            ClipboardHelper.SetText(text);
            return ModuleResult.Copied(text);
        }

        return ModuleResult.Error("Unknown command");
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {
        ModuleArgs.SplitCurrentToken(argument, out var before, out var token);

        if (before.Length == 0)
            return ModuleArgs.SuggestFlags(token, Flags);

        if (before.Equals("b64 ", StringComparison.OrdinalIgnoreCase))
            return ModuleArgs.SuggestFlags(token, B64Flags);

        if (before.Equals("hex ", StringComparison.OrdinalIgnoreCase))
            return ModuleArgs.SuggestFlags(token, HexFlags);

        return Array.Empty<ArgCompletion>();
    }


    private static ModuleResult RunB64(string rest) {
        var space = rest.IndexOf(' ');
        var mode = space < 0 ? rest : rest[..space];
        var text = space < 0 ? "" : rest[(space + 1)..].Trim();

        if (text.Length == 0)
            return ModuleResult.Error("Text required");

        if (mode.Equals("encode", StringComparison.OrdinalIgnoreCase)) {
            var bytes = Encoding.UTF8.GetBytes(text);
            var encoded = Convert.ToBase64String(bytes);
            ClipboardHelper.SetText(encoded);
            return ModuleResult.Copied(encoded);
        }

        if (mode.Equals("decode", StringComparison.OrdinalIgnoreCase)) {
            try {
                var bytes = Convert.FromBase64String(text);
                var decoded = Encoding.UTF8.GetString(bytes);
                ClipboardHelper.SetText(decoded);
                return ModuleResult.Copied(decoded);
            }
            catch {
                return ModuleResult.Error("Invalid Base64");
            }
        }

        return ModuleResult.Error("Unknown mode");
    }


    private static ModuleResult RunHex(string rest) {
        var space = rest.IndexOf(' ');
        var mode = space < 0 ? rest : rest[..space];
        var text = space < 0 ? "" : rest[(space + 1)..].Trim();

        if (text.Length == 0)
            return ModuleResult.Error("Text required");

        if (mode.Equals("encode", StringComparison.OrdinalIgnoreCase)) {
            var bytes = Encoding.UTF8.GetBytes(text);
            var encoded = Convert.ToHexString(bytes).ToLowerInvariant();
            ClipboardHelper.SetText(encoded);
            return ModuleResult.Copied(encoded);
        }

        if (mode.Equals("decode", StringComparison.OrdinalIgnoreCase)) {
            try {
                var clean = text.Replace(" ", "", StringComparison.Ordinal);
                var bytes = Convert.FromHexString(clean);
                var decoded = Encoding.UTF8.GetString(bytes);
                ClipboardHelper.SetText(decoded);
                return ModuleResult.Copied(decoded);
            }
            catch {
                return ModuleResult.Error("Invalid hex");
            }
        }

        return ModuleResult.Error("Unknown mode");
    }
}
