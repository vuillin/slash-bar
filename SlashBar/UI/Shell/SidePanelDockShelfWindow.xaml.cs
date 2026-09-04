using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SlashBar.UI.Shell;

public partial class SidePanelDockShelfWindow : Window {

    public SidePanelDockShelfWindow() {
        InitializeComponent();
    }

    public void Refresh(ObservableCollection<SidePanelDockShelfItem> items) {
        ShelfList.ItemsSource = items;

        if (items.Count == 0) {
            Hide();
            return;
        }

        if (!IsVisible)
            Show();

        UpdateLayout();
        PositionBottomLeft();
    }

    private void PositionBottomLeft() {
        var screen = System.Windows.Forms.Screen.PrimaryScreen
            ?? System.Windows.Forms.Screen.AllScreens[0];
        var area = screen.WorkingArea;

        Left = area.Left;
        Top = area.Bottom - ActualHeight;
    }

    private void ShelfItem_Click(object sender, MouseButtonEventArgs e) {
        if (sender is not FrameworkElement { DataContext: SidePanelDockShelfItem item })
            return;

        e.Handled = true;
        SidePanelDockShelf.Restore(item.Panel);
    }
}
