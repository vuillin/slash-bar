using System.Windows.Input;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class CalendarPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 372;
    public override string DockShelfKey => "calendar";

    private static CalendarPanelWindow? _instance;

    private CalendarPanelWindow() {
        InitializeComponent();
        Width = ShellWidth;
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }

    public static void Toggle() {
        _instance ??= new CalendarPanelWindow();
        _instance.ToggleVisibility();
    }
}
