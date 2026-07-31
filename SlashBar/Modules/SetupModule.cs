using SlashBar.Modules.Setup;

namespace SlashBar.Modules;

public sealed class SetupModule : IModule {

    public string Prefix => "setup";
    public string Name => "Setup";
    public string Description => "Launch an application profile";

    public ModuleResult Execute(string argument) {
        argument = argument.Trim();
        if (argument.Length == 0)
            return ModuleResult.Error("Profile required");

        var name = argument.Split(' ', 2)[0];
        var profile = SetupProfiles.Store.Find(name);
        if (profile is null)
            return ModuleResult.Error("Profile not found");

        if (profile.Steps.Count == 0)
            return ModuleResult.Error("Profile has no steps");

        SetupRunner.Run(profile);
        return ModuleResult.Ok("Setup launched");
    }

    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) {
        ModuleArgs.SplitCurrentToken(argument, out var before, out var token);

        if (before.Length == 0) {
            var flags = SetupProfiles.Store.GetAll()
                .Select(p => new ArgCompletion(p.Name, p.Description))
                .ToArray();
            return ModuleArgs.SuggestFlags(token, flags);
        }

        return Array.Empty<ArgCompletion>();
    }
}
