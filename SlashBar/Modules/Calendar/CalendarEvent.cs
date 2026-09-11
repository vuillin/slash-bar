namespace SlashBar.Modules.Calendar;

public sealed class CalendarEvent {

    public string Id { get; set; } = "";

    public string Title { get; set; } = "";

    public DateTime Date { get; set; }

    public string Repeat { get; set; } = "Never";

    public DateTimeOffset CreatedAt { get; set; }
}