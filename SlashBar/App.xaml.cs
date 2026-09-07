using System.Windows;
using SlashBar.Modules.Awake;

namespace SlashBar;

public partial class App : System.Windows.Application {

    protected override void OnExit(ExitEventArgs e) {
        AwakeSession.Disable();
        base.OnExit(e);
    }
}
