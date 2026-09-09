using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SlashBar.UI.Shell;

public partial class SidePanelHeader : System.Windows.Controls.UserControl {

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SidePanelHeader),
            new PropertyMetadata(null, OnTitleChanged));

    public static readonly DependencyProperty TitleBrushProperty =
        DependencyProperty.Register(
            nameof(TitleBrush),
            typeof(System.Windows.Media.Brush),
            typeof(SidePanelHeader),
            new PropertyMetadata(new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1C, 0x1C, 0x1E))));

    public static readonly DependencyProperty TitleVisibilityProperty =
        DependencyProperty.Register(
            nameof(TitleVisibility),
            typeof(Visibility),
            typeof(SidePanelHeader),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty LeadingContentProperty =
        DependencyProperty.Register(
            nameof(LeadingContent),
            typeof(object),
            typeof(SidePanelHeader));

    public static readonly DependencyProperty HeaderPaddingProperty =
        DependencyProperty.Register(
            nameof(HeaderPadding),
            typeof(Thickness),
            typeof(SidePanelHeader),
            new PropertyMetadata(new Thickness(14, 10, 14, 6)));


    public static readonly RoutedEvent MinimizeClickEvent = EventManager.RegisterRoutedEvent(
        nameof(MinimizeClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelHeader));

    public static readonly RoutedEvent CloseClickEvent = EventManager.RegisterRoutedEvent(
        nameof(CloseClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelHeader));

    public static readonly RoutedEvent HeaderDragEvent = EventManager.RegisterRoutedEvent(
        nameof(HeaderDrag), RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(SidePanelHeader));


    public string? Title {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public System.Windows.Media.Brush TitleBrush {
        get => (System.Windows.Media.Brush)GetValue(TitleBrushProperty);
        set => SetValue(TitleBrushProperty, value);
    }


    public Visibility TitleVisibility {
        get => (Visibility)GetValue(TitleVisibilityProperty);
        private set => SetValue(TitleVisibilityProperty, value);
    }

    public object? LeadingContent {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
    }

    public Thickness HeaderPadding {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }


    public event RoutedEventHandler MinimizeClick {
        add => AddHandler(MinimizeClickEvent, value);
        remove => RemoveHandler(MinimizeClickEvent, value);
    }

    public event RoutedEventHandler CloseClick {
        add => AddHandler(CloseClickEvent, value);
        remove => RemoveHandler(CloseClickEvent, value);
    }

    public event MouseButtonEventHandler HeaderDrag {
        add => AddHandler(HeaderDragEvent, value);
        remove => RemoveHandler(HeaderDragEvent, value);
    }


    public SidePanelHeader() {
        InitializeComponent();
        UpdateTitleVisibility();
    }


    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is SidePanelHeader header)
            header.UpdateTitleVisibility();
    }


    private void UpdateTitleVisibility() =>
        TitleVisibility = string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;


    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (VisualTreeExtensions.FindAncestor<System.Windows.Controls.Primitives.ButtonBase>(e.OriginalSource as DependencyObject) != null)
            return;

        RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton) {
            RoutedEvent = HeaderDragEvent,
            Source = this
        });
    }


    private void Minimize_Click(object sender, RoutedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(MinimizeClickEvent, this));


    private void Close_Click(object sender, RoutedEventArgs e) =>
        RaiseEvent(new RoutedEventArgs(CloseClickEvent, this));
}
