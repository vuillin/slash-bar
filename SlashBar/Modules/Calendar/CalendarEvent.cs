using System.Globalization;
using System.Text.Json.Serialization;

namespace SlashBar.Modules.Calendar;

public sealed class CalendarEvent {

    public string Id { get; set; } = "";

    public string Title { get; set; } = "";

    public DateTime Date { get; set; }

    public string Repeat { get; set; } = "Never";

    public DateTimeOffset CreatedAt { get; set; }

    [JsonIgnore]
    public string DetailLine {
        get {
            var date = Date.ToString("MMM d", CultureInfo.GetCultureInfo("en-US"));
            if (Repeat != "Never" && !string.IsNullOrWhiteSpace(Repeat))
                return $"{date} · {Repeat}";
            return date;
        }
    }
}
