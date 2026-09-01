using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SlashBar.Modules.Weather;
using SlashBar.UI.Shell;

namespace SlashBar;

public partial class WeatherPanelWindow : DockedSidePanelWindow {

    protected override double PanelContentWidth => 400;
    protected override bool ShowsDockTab => false;
    protected override double DockedHeight(double workAreaHeight) => 198;

    protected override double DockedTop(System.Drawing.Rectangle workArea, double height) =>
        workArea.Top + LeftMargin;

    private static WeatherPanelWindow? _instance;

    private readonly DispatcherTimer _refreshTimer;
    private int _loadId;


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

        if (_instance.IsVisible) {
            _instance.AnimateClose();
        } else {
            SidePanelCoordinator.CloseOthersExcept(typeof(WeatherPanelWindow));
            _instance.AnimateOpen();
        }
    }


    public static void CloseIfOpen() {
        if (_instance is { IsVisible: true })
            _instance.AnimateClose();
    }


    static WeatherPanelWindow() {
        SidePanelCoordinator.Register(typeof(WeatherPanelWindow), CloseIfOpen);
    }


    protected override async void OnPanelOpening() {
        await WeatherGeolocator.RequestAccessAsync();

        var locationKey = WeatherClient.BuildLocationKey();
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


    private void LoadWeather(bool showLoadingOnFailure) {
        var id = ++_loadId;
        var locationKey = WeatherClient.BuildLocationKey();

        if (showLoadingOnFailure)
            ShowStatus("Loading…");

        _ = Task.Run(() => {
            try {
                var snapshot = WeatherClient.FetchFresh(locationKey);
                Dispatcher.Invoke(() => {
                    if (id != _loadId)
                        return;
                    ShowSnapshot(snapshot);
                });
            }
            catch {
                Dispatcher.Invoke(() => {
                    if (id != _loadId)
                        return;
                    if (showLoadingOnFailure && ContentCard.Visibility != Visibility.Visible)
                        ShowStatus("Weather unavailable");
                });
            }
        });
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
