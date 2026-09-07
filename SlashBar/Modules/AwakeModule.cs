using SlashBar.Modules.Awake;

namespace SlashBar.Modules;

public sealed class AwakeModule : IModule {

    public string Prefix => "awake";
    public string Name => "Awake";
    public string Description => "Keep the computer awake";


    public ModuleResult Execute(string argument) {
        if (argument.Trim().Length > 0)
            return ModuleResult.Error("No arguments");

        if (AwakeSession.IsActive) {
            AwakeSession.Disable();
            AwakeToast.Hide();
            return ModuleResult.Ok("Awake off");
        }

        AwakeSession.Enable();
        AwakeToast.Show();
        // Sticky toast is the feedback : avoid a second ephemeral toast
        return ModuleResult.None;
    }
}