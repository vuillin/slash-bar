using System.Windows;
using System.Windows.Input;
using SlashBar.Modules.Color;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class ColorPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 390;

    protected override double DockedHeight(double workAreaHeight) =>
        ColorHistory.Store.GetAll().Count > 0 ? 338 : 268;

    private static ColorPanelWindow? _instance;

    private bool _colorLocked;


    private ColorPanelWindow() {
        InitializeComponent();
        Width = ShellWidth;

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
        SubscribeHistory();
        RefreshHistory();
    }

    protected override void OnPanelOpened() =>
        SyncMagnifierVisibility();

    protected override void OnPanelClosing() =>
        DisablePickMode();

    protected override void OnResetToDockCompleted() {
        if (!_pickModeActive)
            EnablePickMode();
    }
}
