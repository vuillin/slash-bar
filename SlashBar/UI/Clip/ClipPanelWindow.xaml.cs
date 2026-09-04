using System.Windows;
using System.Windows.Input;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class ClipPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 372;

    public override string DockShelfKey => "clip";

    private static ClipPanelWindow? _instance;

    private bool _historySubscribed;

    private ClipPanelWindow() {
        InitializeComponent();
        Width = ShellWidth;
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }

    public static void Toggle() {
        _instance ??= new ClipPanelWindow();
        _instance.ToggleVisibility();
    }

    protected override void OnPanelOpening() {
        SubscribeHistory();
        RefreshHistory();
    }
}
