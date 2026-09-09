using System.Windows;
using SlashBar.Modules.Awake;
using SlashBar.Modules.Clipboard;
using SlashBar.Modules.Color;
using SlashBar.Modules.Memo;
using SlashBar.Modules.Pin;

namespace SlashBar;

public partial class App : System.Windows.Application {

    protected override void OnExit(ExitEventArgs e) {
        AwakeSession.Disable();
        PinSession.UnpinAll();
        ClipboardHistory.Store.Flush();
        MemoBook.Store.Flush();
        ColorHistory.Store.Flush();
        base.OnExit(e);
    }
}
