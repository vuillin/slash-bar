using System.Text;

namespace SlashBar.Modules;

public sealed class GenModule : IModule {


    private static readonly ArgCompletion[] Flags = [
        new("b64", "Base64 encodage/décodage"),
        new("hex", "Hexadécimal encodage/décodage"),

        new("date", "Date du jour (jj-mm-aaaa)"),
        new("time", "Heure actuelle (hh:mm:ss)"),
        new("timestamp", "Timestamp Unix (secondes)")
    ];

    private static readonly ArgCompletion[] B64Flags = [
        new("encode", "Encoder en Base64"),
        new("decode", "Décoder depuis Base64")
    ];

    private static readonly ArgCompletion[] HexFlags = [
        new("encode", "Encoder en hexadécimal"),
        new("decode", "Décoder depuis l'hexadécimal")
    ];

    public string Prefix => "gen";
    public string Name => "Générateur";
    public string Description => "Génère une valeur (presse-papiers)";


    public void Execute(string argument) {

        argument = argument.Trim();
        if (argument.Length == 0)
            return;

        var space = argument.IndexOf(' ');
        var cmd = space < 0 ? argument : argument[..space];
        var rest = space < 0 ? "" : argument[(space + 1)..].Trim();

        if (cmd.Equals("b64", StringComparison.OrdinalIgnoreCase)) {
            RunB64(rest);
            return;
        }

        if (cmd.Equals("hex", StringComparison.OrdinalIgnoreCase)) {
            RunHex(rest);
            return;
        }

        if (cmd.Equals("date", StringComparison.OrdinalIgnoreCase)) {
            ClipboardHelper.SetText(DateTime.Now.ToString("dd-MM-yyyy"));
            return;
        }
        if (cmd.Equals("time", StringComparison.OrdinalIgnoreCase)) {
            ClipboardHelper.SetText(DateTime.Now.ToString("HH:mm:ss"));
            return;
        }
        if (cmd.Equals("timestamp", StringComparison.OrdinalIgnoreCase)) {
            ClipboardHelper.SetText(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            return;
        }
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {

        ModuleArgs.SplitCurrentToken(argument, out var before, out var token);

        // niveau 1
        if (before.Length == 0)
            return ModuleArgs.SuggestFlags(token, Flags);

        // niveau 2
        if (before.Equals("b64 ", StringComparison.OrdinalIgnoreCase))
            return ModuleArgs.SuggestFlags(token, B64Flags);

        if (before.Equals("hex ", StringComparison.OrdinalIgnoreCase))
            return ModuleArgs.SuggestFlags(token, HexFlags);

        return Array.Empty<ArgCompletion>();
    }
    

    private static void RunB64(string rest) {

        var space = rest.IndexOf(' ');
        var mode = space < 0 ? rest : rest[..space];
        var text = space < 0 ? "" : rest[(space + 1)..].Trim();

        if (text.Length == 0)
            return;

        if (mode.Equals("encode", StringComparison.OrdinalIgnoreCase)) {
            
            var bytes = Encoding.UTF8.GetBytes(text);
            ClipboardHelper.SetText(Convert.ToBase64String(bytes));
            return;
        }

        if (mode.Equals("decode", StringComparison.OrdinalIgnoreCase)) {

            try {
                var bytes = Convert.FromBase64String(text);
                ClipboardHelper.SetText(Encoding.UTF8.GetString(bytes));
            }
            catch {
                // base64 invalide → ignore
            }
        }
    }


    private static void RunHex(string rest) {

        var space = rest.IndexOf(' ');
        var mode = space < 0 ? rest : rest[..space];
        var text = space < 0 ? "" : rest[(space + 1)..].Trim();

        if (text.Length == 0)
            return;

        if (mode.Equals("encode", StringComparison.OrdinalIgnoreCase)) {
            
            var bytes = Encoding.UTF8.GetBytes(text);
            ClipboardHelper.SetText(Convert.ToHexString(bytes).ToLowerInvariant());
            return;
        }

        if (mode.Equals("decode", StringComparison.OrdinalIgnoreCase)) {

            try {
                var clean = text.Replace(" ", "", StringComparison.Ordinal);
                var bytes = Convert.FromHexString(clean);
                ClipboardHelper.SetText(Encoding.UTF8.GetString(bytes));
            }
            catch {
                // hex invalide → ignore
            }
        }
    }
}