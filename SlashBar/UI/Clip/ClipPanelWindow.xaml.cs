using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SlashBar;

public partial class ClipPanelWindow : Window {

    private const double PanelWidth = 340;
    private const double LeftMargin = 14;
    private const double TabWidth = 32;

    // panneau hors écran, onglet visible au bord
    private static double CollapsedX => -(LeftMargin + PanelWidth);

    private static ClipPanelWindow? _instance;

    private bool _isCollapsed;
    private bool _isAnimating;
    private bool _isDetached;
    private bool _historySubscribed;

    private ClipPanelWindow() {
        InitializeComponent();
        Width = LeftMargin + PanelWidth + TabWidth;
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }

    public static void Toggle() {
        _instance ??= new ClipPanelWindow();

        if (_instance.IsVisible)
            _instance.AnimateClose();
        else
            _instance.AnimateOpen();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) =>
        AnimateClose();

    private void ResetButton_Click(object sender, RoutedEventArgs e) =>
        ResetToDock();

    private void DockButton_Click(object sender, RoutedEventArgs e) {
        if (_isDetached)
            return;

        if (_isCollapsed)
            AnimateExpand();
        else
            AnimateCollapse();
    }

    private void HeaderBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (_isCollapsed || _isAnimating)
            return;

        // ne pas draguer depuis les feux de signalisation
        if (FindAncestor<System.Windows.Controls.Button>(e.OriginalSource as DependencyObject) != null)
            return;

        if (!_isDetached)
            EnterDetachedMode();

        DragMove();
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject {
        while (current != null) {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
