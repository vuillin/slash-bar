using System.Windows;
using SlashBar.Modules.Awake;
using SlashBar.Modules.Pin;

namespace SlashBar;

public partial class App : System.Windows.Application {

    protected override void OnExit(ExitEventArgs e) {
        AwakeSession.Disable();
        PinSession.UnpinAll();
        base.OnExit(e);
    }
}
