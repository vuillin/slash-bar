using System.Windows;
using System.Windows.Input;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class MemoPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 380;

    public override string DockShelfKey => "memo";

    protected override double DockedHeight(double workAreaHeight) => 420;

    private static MemoPanelWindow? _instance;

    private MemoPanelWindow() {
        InitializeComponent();
        Width = ShellWidth;
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }

    protected override void OnPanelOpening() {
        SubscribeList();
        RefreshList();
    }

    public static void Toggle() {
        _instance ??= new MemoPanelWindow();
        _instance.ToggleVisibility();
    }
}
