using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class CalendarPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 818;
    public override string DockShelfKey => "calendar";

    protected override double DockedHeight(double workAreaHeight) => workAreaHeight * 0.52;

    private static readonly CultureInfo UiCulture = new("en-US");
    private static readonly SolidColorBrush CurrentMonthBrush = Freeze(0x1C, 0x1C, 0x1E);
    private static readonly SolidColorBrush OtherMonthBrush = Freeze(0xC7, 0xC7, 0xCC);
    private static readonly SolidColorBrush SelectedFillBrush = Freeze(0xFF, 0x3B, 0x30);
    private static readonly SolidColorBrush SelectedTextBrush = Freeze(0xFF, 0xFF, 0xFF);

    private static CalendarPanelWindow? _instance;

    private DateTime _viewMonth;
    private DateTime _selectedDate;


    private CalendarPanelWindow() {
        InitializeComponent();

        var today = DateTime.Today;
        _selectedDate = today;
        _viewMonth = new DateTime(today.Year, today.Month, 1);

        RefreshHeader();
        RefreshMonthLabel();
        RebuildMonthGrid();

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


    private void RefreshHeader() {
        var dayTitle = (TextBlock)PanelHeader.LeadingContent!;
        dayTitle.Text = _selectedDate
            .ToString("dddd", UiCulture)
            .ToUpperInvariant();

        DayNumber.Inlines.Clear();
        DayNumber.Inlines.Add(new Run(_selectedDate.ToString("MMMM d,", UiCulture)) {
            FontWeight = FontWeights.Bold
        });
        DayNumber.Inlines.Add(new Run($" {_selectedDate.Year}"));
    }


    private void RebuildMonthGrid() {
        MonthGrid.Children.Clear();

        var first = _viewMonth;
        var start = first.AddDays(-(int)first.DayOfWeek);

        for (var i = 0; i < 42; i++) {
            var date = start.AddDays(i);
            MonthGrid.Children.Add(CreateDayCell(date));
        }
    }


    private void RefreshMonthLabel() {
        MonthLabel.Text = _viewMonth.ToString("MMMM", UiCulture);
        YearLabel.Text = _viewMonth.Year.ToString();
    }


    private UIElement CreateDayCell(DateTime date) {
        var isCurrentMonth = date.Month == _viewMonth.Month;
        var isSelected = date.Date == _selectedDate.Date;

        var label = new TextBlock {
            Text = date.Day.ToString(UiCulture),
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI"),
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Foreground = isSelected
                ? SelectedTextBrush
                : isCurrentMonth ? CurrentMonthBrush : OtherMonthBrush,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        var cell = new Border {
            Width = 26,
            Height = 26,
            CornerRadius = new CornerRadius(13),
            Background = isSelected ? SelectedFillBrush : System.Windows.Media.Brushes.Transparent,
            Cursor = System.Windows.Input.Cursors.Hand,
            Tag = date.Date,
            Child = label,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0)
        };

        cell.MouseLeftButtonUp += DayCell_Click;
        return cell;
    }


    private void DayCell_Click(object sender, MouseButtonEventArgs e) {
        if (sender is not Border { Tag: DateTime date })
            return;

        _selectedDate = date.Date;
        if (_selectedDate.Month != _viewMonth.Month || _selectedDate.Year != _viewMonth.Year)
            _viewMonth = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);

        RefreshHeader();
        RebuildMonthGrid();
        RefreshMonthLabel();
    }


    private void PrevMonthButton_Click(object sender, RoutedEventArgs e) {
        _viewMonth = _viewMonth.AddMonths(-1);
        RefreshMonthLabel();
        RebuildMonthGrid();
    }

    private void NextMonthButton_Click(object sender, RoutedEventArgs e) {
        _viewMonth = _viewMonth.AddMonths(1);
        RefreshMonthLabel();
        RebuildMonthGrid();
    }

    private void MonthLabelButton_Click(object sender, RoutedEventArgs e) {
        var today = DateTime.Today;
        _selectedDate = today;
        _viewMonth = new DateTime(today.Year, today.Month, 1);
        RefreshMonthLabel();
        RefreshHeader();
        RebuildMonthGrid();
    }


    private static SolidColorBrush Freeze(byte r, byte g, byte b) {
        var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
