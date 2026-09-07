using System.Globalization;
using System.Text.RegularExpressions;

namespace SlashBar.Modules.Awake;

public static class AwakeDuration {

    public static TimeSpan Max { get; } = TimeSpan.FromHours(24);

    private static readonly Regex Pattern = new(
        @"^(?<n>\d+)(?<u>[mh])?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);


    public static bool TryParse(string token, out TimeSpan duration) {
        duration = default;

        var m = Pattern.Match(token.Trim());
        if (!m.Success)
            return false;

        if (!int.TryParse(m.Groups["n"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var n)
            || n <= 0)
            return false;

        var unit = m.Groups["u"].Value;
        duration = unit.Equals("h", StringComparison.OrdinalIgnoreCase)
            ? TimeSpan.FromHours(n)
            : TimeSpan.FromMinutes(n);

        return duration <= Max;
    }


    public static string FormatRemaining(TimeSpan remaining) {
        if (remaining < TimeSpan.Zero)
            remaining = TimeSpan.Zero;

        if (remaining.TotalHours >= 1) {
            var h = (int)remaining.TotalHours;
            var m = remaining.Minutes;
            return m > 0 ? $"{h}h {m}m" : $"{h}h";
        }

        var minutes = (int)Math.Ceiling(remaining.TotalMinutes);
        if (minutes >= 1)
            return $"{minutes}m";

        var seconds = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
        return $"{seconds}s";
    }
}