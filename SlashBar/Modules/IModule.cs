namespace SlashBar.Modules;

// Préfixe + Execute. Toute classe concrète dans ce dossier est chargée automatiquement.
public interface IModule
{
    string Prefix { get; }
    string Name { get; }
    string Description { get; }

    // argument = tout ce qui suit le préfixe ("f chatgpt" → "chatgpt")
    void Execute(string argument);
}
