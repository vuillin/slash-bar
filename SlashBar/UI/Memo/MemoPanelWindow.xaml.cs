using System.Windows;
using System.Windows.Input;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class MemoPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 380;

    private static MemoPanelWindow? _instance;

    private MemoPanelWindow() {
        InitializeComponent();
        Width = LeftMargin + PanelContentWidth + TabWidth;
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

        if (_instance.IsVisible) {
            _instance.AnimateClose();
        } else {
            SidePanelCoordinator.CloseOthersExcept(typeof(MemoPanelWindow));
            _instance.AnimateOpen();
        }
    }

    public static void CloseIfOpen() {
        if (_instance is { IsVisible: true })
            _instance.AnimateClose();
    }

    static MemoPanelWindow() {
        SidePanelCoordinator.Register(typeof(MemoPanelWindow), CloseIfOpen);
    }
}
