namespace SlashBar.Modules;

public sealed class ClipModule : IModule {

    public string Prefix => "clip";
    public string Name => "Presse-papiers";
    public string Description => "Historique du presse-papiers";

    public void Execute(string argument) {
        SlashBar.ClipPanelWindow.Toggle();
    }
}
