using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using SlashBar.Modules.Calendar;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class CalendarPanelWindow : DockedSidePanelWindow {

    private const double BaseContentWidth = 818;
    private const double EventsDrawerExpandedWidth = 280;

    protected override double PanelContentWidth =>
        BaseContentWidth + (_eventsDrawerOpen ? EventsDrawerExpandedWidth : 0);

    public override string DockShelfKey => "calendar";

    protected override double DockedHeight(double workAreaHeight) => workAreaHeight * 0.416;

    private static readonly CultureInfo UiCulture = new("en-US");
    private static readonly SolidColorBrush CurrentMonthBrush = Freeze(0x1C, 0x1C, 0x1E);
    private static readonly SolidColorBrush OtherMonthBrush = Freeze(0xC7, 0xC7, 0xCC);
    private static readonly SolidColorBrush SelectedFillBrush = Freeze(0xFF, 0x3B, 0x30);
    private static readonly SolidColorBrush SelectedTextBrush = Freeze(0xFF, 0xFF, 0xFF);

    private static readonly SolidColorBrush TimelineLineBrush = Freeze(0xE5, 0xE5, 0xEA);
    private static readonly SolidColorBrush TimelineHourBrush = Freeze(0x8E, 0x8E, 0x93);
    private static readonly SolidColorBrush EventDayBrush = Freeze(0xCA, 0x73, 0xDF);
    private static readonly SolidColorBrush EventDayMutedBrush = Freeze(0xE5, 0xC4, 0xEF);

    private static CalendarPanelWindow? _instance;

    private const double HourRowHeight = 52;
    private DispatcherTimer? _nowTimer;

    private DateTime _viewMonth;
    private DateTime _selectedDate;

    private bool _eventsDrawerOpen;
    private bool _eventsDrawerAnimating;


    private CalendarPanelWindow() {
        InitializeComponent();

        var today = DateTime.Today;
        _selectedDate = today;
        _viewMonth = new DateTime(today.Year, today.Month, 1);

        RefreshHeader();
        RefreshMonthLabel();
        RebuildMonthGrid();
        BuildDayTimeline();

        RefreshNowIndicator();
        _nowTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _nowTimer.Tick += (_, _) => RefreshNowIndicator();
        _nowTimer.Start();

        CalendarBook.Store.Changed += () => Dispatcher.Invoke(() => {
            RefreshDayEvents();
            RebuildMonthGrid();
        });

        Width = ShellWidth;
        PreviewMouseLeftButtonDown += CalendarPanel_PreviewMouseLeftButtonDown;
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }


    public static void Toggle() {
        _instance ??= new CalendarPanelWindow();
        _instance.ToggleVisibility();
    }


    protected override void OnPanelOpening() {
        base.OnPanelOpening();
        RefreshDayEvents();
        RefreshNowIndicator();
        UpdateLayout();
        if (!ScrollNowIndicatorIntoView()) {
            Dispatcher.BeginInvoke(() => {
                UpdateLayout();
                ScrollNowIndicatorIntoView();
            }, DispatcherPriority.Loaded);
        }
    }


    private void ViewEventsLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        e.Handled = true;
        ToggleEventsDrawer();
    }


    private void ToggleEventsDrawer() {
        if (_eventsDrawerAnimating || IsAnimating)
            return;

        AnimateEventsDrawer(!_eventsDrawerOpen);
    }


    private void AnimateEventsDrawer(bool open) {
        _eventsDrawerAnimating = true;

        var from = EventsDrawer.ActualWidth > 0 ? EventsDrawer.ActualWidth : EventsDrawer.Width;
        var to = open ? EventsDrawerExpandedWidth : 0;
        var duration = TimeSpan.FromMilliseconds(280);
        var ease = new QuadraticEase {
            EasingMode = open ? EasingMode.EaseOut : EasingMode.EaseIn
        };

        var drawerAnim = new DoubleAnimation(from, to, duration) { EasingFunction = ease };

        var widthFrom = ActualWidth > 0 ? ActualWidth : Width;
        var widthTo = LeftMargin + BaseContentWidth + to
                      + ((Thickness)System.Windows.Application.Current.FindResource("SidePanelShadowMargin")).Right;
        var windowAnim = new DoubleAnimation(widthFrom, widthTo, duration) { EasingFunction = ease };

        windowAnim.Completed += (_, _) => {
            _eventsDrawerOpen = open;
            _eventsDrawerAnimating = false;

            EventsDrawer.BeginAnimation(WidthProperty, null);
            EventsDrawer.Width = to;

            BeginAnimation(WidthProperty, null);
            Width = ShellWidth;

            ViewEventsLabel.Text = open ? "Hide events" : "View events";
        };

        EventsDrawer.BeginAnimation(WidthProperty, drawerAnim);
        BeginAnimation(WidthProperty, windowAnim);
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

        RefreshDayEvents();
    }


    private void RebuildMonthGrid() {
        MonthGrid.Children.Clear();

        var first = _viewMonth;
        var start = first.AddDays(-(int)first.DayOfWeek);

        for (var i = 0; i < 42; i++) {
            var date = start.AddDays(i);
            MonthGrid.Children.Add(CreateDayCell(date, CalendarBook.Store.HasAnyOn(date)));
        }
    }


    private void RefreshMonthLabel() {
        MonthLabel.Text = _viewMonth.ToString("MMMM", UiCulture);
        YearLabel.Text = _viewMonth.Year.ToString();
    }


    private void RefreshNowIndicator() {
        if (_selectedDate.Date != DateTime.Today) {
            NowIndicator.Visibility = Visibility.Collapsed;
            return;
        }

        var now = DateTime.Now;
        var top = (now.Hour + now.Minute / 60.0) * HourRowHeight
                + HourRowHeight / 2.0
                - NowIndicator.Height / 2.0;

        NowIndicator.Margin = new Thickness(0, top, 0, 0);
        NowIndicatorText.Text = now.ToString("h:mm", UiCulture);
        NowIndicator.Visibility = Visibility.Visible;
    }


    private UIElement CreateDayCell(DateTime date, bool hasEvents) {
        var isCurrentMonth = date.Month == _viewMonth.Month;
        var isSelected = date.Date == _selectedDate.Date;

        System.Windows.Media.Brush foreground;
        if (isSelected)
            foreground = SelectedTextBrush;
        else if (hasEvents)
            foreground = isCurrentMonth ? EventDayBrush : EventDayMutedBrush;
        else
            foreground = isCurrentMonth ? CurrentMonthBrush : OtherMonthBrush;

        var label = new TextBlock {
            Text = date.Day.ToString(UiCulture),
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI"),
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Foreground = foreground,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        var cell = new Border {
            Width = 23,
            Height = 23,
            CornerRadius = new CornerRadius(11.5),
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
        RefreshNowIndicator();
        if (_selectedDate.Date == DateTime.Today)
            Dispatcher.BeginInvoke(() => ScrollNowIndicatorIntoView(),
                DispatcherPriority.Loaded);
    }


    private void BuildDayTimeline() {
        DayTimeline.Children.Clear();

        for (var hour = 0; hour < 24; hour++) {
            var time = new DateTime(2000, 1, 1, hour, 0, 0);

            var row = new Grid {
                Height = 52
            };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var hourText = new TextBlock {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI"),
                Foreground = TimelineHourBrush,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
                IsHitTestVisible = false
            };

            if (hour == 12) {
                hourText.Inlines.Add(new Run("Noon") {
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold
                });
            }
            else {
                hourText.Inlines.Add(new Run(time.ToString("%h", UiCulture)) {
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold
                });
                hourText.Inlines.Add(new Run($" {time.ToString("tt", UiCulture)}") {
                    FontSize = 11,
                    FontWeight = FontWeights.Normal,
                    BaselineAlignment = BaselineAlignment.Center
                });
            }

            System.Windows.Controls.Grid.SetColumn(hourText, 0);

            var line = new Border {
                Height = 1,
                Background = TimelineLineBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            };
            System.Windows.Controls.Grid.SetColumn(line, 1);

            row.Children.Add(hourText);
            row.Children.Add(line);
            DayTimeline.Children.Add(row);
        }
    }


    private bool ScrollNowIndicatorIntoView() {
        if (_selectedDate.Date != DateTime.Today)
            return false;

        if (DayTimelineScroll.ViewportHeight <= 0)
            return false;

        var now = DateTime.Now;
        var indicatorY = (now.Hour + now.Minute / 60.0) * HourRowHeight
                        + HourRowHeight / 2.0;

        var target = indicatorY - DayTimelineScroll.ViewportHeight / 2.0;
        target = Math.Max(0, Math.Min(target, DayTimelineScroll.ScrollableHeight));

        DayTimelineScroll.ScrollToVerticalOffset(target);
        return true;
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
        RefreshNowIndicator();
        Dispatcher.BeginInvoke(() => ScrollNowIndicatorIntoView(),
            DispatcherPriority.Loaded);
    }


    private void RepeatRow_Click(object sender, RoutedEventArgs e) {
        if (RepeatRow.ContextMenu == null)
            return;

        var current = RepeatValueText.Text;
        foreach (var obj in RepeatMenu.Items) {
            if (obj is System.Windows.Controls.MenuItem item)
                item.IsChecked = item.Header as string == current;
        }

        RepeatRow.ContextMenu.PlacementTarget = RepeatRow;
        RepeatRow.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        RepeatRow.ContextMenu.IsOpen = true;
    }

    private void RepeatMenuItem_Click(object sender, RoutedEventArgs e) {
        if (sender is not System.Windows.Controls.MenuItem { Header: string label })
            return;

        RepeatValueText.Text = label;
    }


    private void CalendarPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (!EventTitleBox.IsKeyboardFocusWithin)
            return;

        if (e.OriginalSource is DependencyObject source
            && IsDescendant(EventTitleBox, source))
            return;

        Keyboard.ClearFocus();
    }

    private static bool IsDescendant(DependencyObject parent, DependencyObject? node) {
        while (node != null) {
            if (ReferenceEquals(node, parent))
                return true;
            node = VisualTreeHelper.GetParent(node);
        }
        return false;
    }


    private void AddEventButton_Click(object sender, RoutedEventArgs e) {
        var title = EventTitleBox.Text;
        var repeat = RepeatValueText.Text;
        var date = _selectedDate;

        var ok = CalendarBook.Store.Add(title, date, repeat);
        if (!ok)
            return;

        EventTitleBox.Text = "";
        RepeatValueText.Text = "Never";
        Keyboard.ClearFocus();
    }


    private void RefreshDayEvents() {
        DayEventPills.ItemsSource = CalendarBook.Store.GetOccurringOn(_selectedDate);
    }


    private static SolidColorBrush Freeze(byte r, byte g, byte b) {
        var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
