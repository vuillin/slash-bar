namespace SlashBar.Modules;

public sealed class CalendarModule : IModule {

    public string Prefix => "calendar";
    public string Name => "Calendar";
    public string Description => "Open the calendar";

    public ModuleResult Execute(string argument) {
        SlashBar.CalendarPanelWindow.Toggle();
        return ModuleResult.None;
    }
}
