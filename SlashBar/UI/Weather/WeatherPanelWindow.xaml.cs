using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SlashBar.Modules.Weather;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class WeatherPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 400;
    public override string DockShelfKey => "weather";
    protected override double DockedHeight(double workAreaHeight) => 198;

    protected override double DockedTop(System.Drawing.Rectangle workArea, double height) =>
        workArea.Top + LeftMargin;

    private static WeatherPanelWindow? _instance;

    private readonly DispatcherTimer _refreshTimer;
    private int _loadId;

    private System.Windows.Controls.TextBlock PlaceText =>
        (System.Windows.Controls.TextBlock)((System.Windows.Controls.StackPanel)PanelHeader.LeadingContent!).Children[0];

    private System.Windows.Controls.Image ConditionIcon =>
        (System.Windows.Controls.Image)((System.Windows.Controls.StackPanel)PanelHeader.LeadingContent!).Children[1];


    private WeatherPanelWindow() {
        InitializeComponent();
        Width = ShellWidth;
        _refreshTimer = new DispatcherTimer {
            Interval = WeatherCacheStore.ForecastTtl
        };
        _refreshTimer.Tick += (_, _) => LoadWeather(showLoadingOnFailure: false);
        PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape)
                AnimateClose();
        };
    }


    public static void Toggle() {
        _instance ??= new WeatherPanelWindow();
        _instance.ToggleVisibility();
    }


    protected override async void OnPanelOpening() {
        await WeatherGeolocator.RequestAccessAsync();

        var locationKey = await WeatherClient.BuildLocationKeyAsync();
        var hasCache = WeatherCacheStore.TryRead(locationKey, out var cached);

        if (hasCache) {
            ShowSnapshot(cached!.Snapshot);
            if (WeatherCacheStore.IsFresh(cached))
                return;
        }

        LoadWeather(showLoadingOnFailure: !hasCache);
    }


    protected override void OnPanelOpened() {
        _refreshTimer.Start();
    }


    protected override void OnPanelClosing() {
        _refreshTimer.Stop();
        _loadId++;
    }


    private async void LoadWeather(bool showLoadingOnFailure) {
        var id = ++_loadId;

        if (showLoadingOnFailure)
            ShowStatus("Loading…");

        try {
            var locationKey = await WeatherClient.BuildLocationKeyAsync();
            var snapshot = await WeatherClient.FetchFreshAsync(locationKey);
            if (id != _loadId)
                return;
            ShowSnapshot(snapshot);
        }
        catch {
            if (id != _loadId)
                return;
            if (showLoadingOnFailure && ContentCard.Visibility != Visibility.Visible)
                ShowStatus("Weather unavailable");
        }
    }


    private void ShowStatus(string message) {
        StatusText.Text = message;
        StatusText.Visibility = Visibility.Visible;
        ContentCard.Visibility = Visibility.Collapsed;
        PlaceText.Text = "";
        ConditionIcon.Source = null;
    }


    private void ShowSnapshot(WeatherSnapshot snapshot) {
        PlaceText.Text = snapshot.Place;
        ConditionIcon.Source = LoadIcon(snapshot.WeatherCode, snapshot.IsDay);
        TempText.Text = snapshot.Temperature;
        ConditionText.Text = snapshot.Condition;
        HighText.Text = snapshot.High;
        LowText.Text = snapshot.Low;
        HourList.ItemsSource = snapshot.Hours;

        StatusText.Visibility = Visibility.Collapsed;
        ContentCard.Visibility = Visibility.Visible;
    }


    private static BitmapImage LoadIcon(int code, bool isDay) {
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = WeatherCodes.IconUri(code, isDay);
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
