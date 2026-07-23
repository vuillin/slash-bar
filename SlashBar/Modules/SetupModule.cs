using SlashBar.Modules.Setup;

namespace SlashBar.Modules;

public sealed class SetupModule : IModule {


    private static readonly ArgCompletion[] Flags = SetupProfiles.All
        .Select(p => new ArgCompletion(p.Name, p.Description))
        .ToArray();


    public string Prefix => "setup";
    public string Name => "Setup";
    public string Description => "Lance un profil d'applications";


    public void Execute(string argument) {

        argument = argument.Trim();
        if (argument.Length == 0)
            return;
        
        var name = argument.Split(' ', 2)[0];
        var profile = SetupProfiles.Find(name);
        if (profile is null)
            return;

        SetupRunner.Run(profile);
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {

        ModuleArgs.SplitCurrentToken(argument, out var before, out var token);

        // niveau 1
        if (before.Length == 0)
            return ModuleArgs.SuggestFlags(token, Flags);

        return Array.Empty<ArgCompletion>();
    }
}