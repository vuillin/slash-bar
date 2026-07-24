namespace SlashBar.Modules;

public sealed class ColorModule : IModule {

    public string Prefix => "color";
    public string Name => "Color Picker";
    public string Description => "Ouvre le sélecteur de couleur";

    public void Execute(string argument) {

        SlashBar.ColorPanelWindow.Toggle();
        
    }
}