namespace SlashBar.Modules;

/// <summary>
/// Détecte et normalise une url
/// </summary>
public static class UrlHelper {

    /// <summary>
    /// true si <paramref name="input"/> ressemble à une url. ex: "github.com" → "https://github.com/"
    /// </summary>
    public static bool TryNormalize(string input, out string url) {

        url = "";
        input = input.Trim();

        if (input.Length == 0 || input.Contains(' '))
            return false;

        var candidate = input;
        if (!candidate.Contains("://", StringComparison.Ordinal))
            candidate = "https://" + candidate;

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            return false;
        
        if (uri.Scheme is not ("http" or "https"))
            return false;

        if (!uri.Host.Contains('.') && !uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return false;

        url = uri.ToString();
        return true;
    }
}