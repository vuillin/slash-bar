using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class ColorPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 400;

    private static ColorPanelWindow? _instance;

    private bool _colorLocked;


    private ColorPanelWindow() {
        InitializeComponent();
        Width = LeftMargin + PanelContentWidth + TabWidth;

        MouseEnter += (_, _) => SyncMagnifierVisibility();
        MouseLeave += (_, _) => SyncMagnifierVisibility();

        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape) {
                AnimateClose();
                e.Handled = true;
                return;
            }

            if (!_pickModeActive)
                return;

            switch (e.Key) {
                case Key.Left:  NudgeCursor(-1, 0); e.Handled = true; break;
                case Key.Right: NudgeCursor(1,  0); e.Handled = true; break;
                case Key.Up:    NudgeCursor(0, -1); e.Handled = true; break;
                case Key.Down:  NudgeCursor(0,  1); e.Handled = true; break;
            }
        };
    }


    public static void Toggle() {
        _instance ??= new ColorPanelWindow();

        if (_instance.IsVisible) {
            _instance.AnimateClose();
        } else {
            SidePanelCoordinator.CloseOthersExcept(typeof(ColorPanelWindow));
            _instance.AnimateOpen();
        }
    }

    public static void CloseIfOpen() {
        if (_instance is { IsVisible: true })
            _instance.AnimateClose();
    }

    static ColorPanelWindow() {
        SidePanelCoordinator.Register(typeof(ColorPanelWindow), CloseIfOpen);
    }


    protected override void OnPanelOpening() {
        _colorLocked = false;
        EnablePickMode();
    }

    protected override void OnPanelOpened() =>
        SyncMagnifierVisibility();

    protected override void OnPanelClosing() =>
        DisablePickMode();

    protected override void OnPanelCollapsed() =>
        DisablePickMode();

    protected override void OnPanelExpanding() =>
        EnablePickMode();

    protected override void OnPanelExpanded() =>
        SyncMagnifierVisibility();

    protected override void OnResetToDockCompleted() {
        if (!_pickModeActive)
            EnablePickMode();
    }
}
