using System.Windows;
using System.Windows.Input;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class ClipPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 340;

    private static ClipPanelWindow? _instance;

    private bool _historySubscribed;

    private ClipPanelWindow() {
        InitializeComponent();
        Width = LeftMargin + PanelContentWidth + TabWidth;
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }

    public static void Toggle() {
        _instance ??= new ClipPanelWindow();

        if (_instance.IsVisible) {
            _instance.AnimateClose();
        } else {
            SidePanelCoordinator.CloseOthersExcept(typeof(ClipPanelWindow));
            _instance.AnimateOpen();
        }
    }

    public static void CloseIfOpen() {
        if (_instance is { IsVisible: true })
            _instance.AnimateClose();
    }

    static ClipPanelWindow() {
        SidePanelCoordinator.Register(typeof(ClipPanelWindow), CloseIfOpen);
    }

    protected override void OnPanelOpening() {
        SubscribeHistory();
        RefreshHistory();
    }
}
