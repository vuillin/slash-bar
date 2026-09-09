using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SlashBar.Modules.Color;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class ColorPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 390;

    public override string DockShelfKey => "color";

    protected override double DockedHeight(double workAreaHeight) =>
        ColorHistory.Store.GetAll().Count > 0 ? 338 : 268;

    private static ColorPanelWindow? _instance;

    private bool _colorLocked;

    private ToggleButton EyedropperToggle => (ToggleButton)PanelHeader.LeadingContent!;


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
        _instance.ToggleVisibility();
    }


    protected override void OnPanelOpening() {
        _colorLocked = false;
        EyedropperToggle.IsChecked = true;
        EnablePickMode();
        SubscribeHistory();
        RefreshHistory();
    }

    protected override void OnPanelOpened() =>
        SyncMagnifierVisibility();

    protected override void OnPanelClosing() =>
        DisablePickMode();

    private void EyedropperToggle_Changed(object sender, RoutedEventArgs e) {
        if (!IsVisible)
            return;

        if (EyedropperToggle.IsChecked == true)
            EnablePickMode();
        else
            DisablePickMode();
    }
}
