using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

    private int _loadId;


    private WeatherPanelWindow() {
        InitializeComponent();
        Width = ChromeWidth;
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


    protected override void OnPanelOpening() {
        var id = ++_loadId;
        ShowStatus("Loading…");

        Task.Run(() => {
            try {
                var snapshot = WeatherClient.Fetch();
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
