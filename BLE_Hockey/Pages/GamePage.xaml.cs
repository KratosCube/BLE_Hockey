namespace BLE_Hockey.Pages;

public partial class GamePage : ContentPage
{
    public readonly DeviceOrientationService _androidDeviceOrientationService;
    public BLEService BluetoothLEService { get; private set; }
    public GamePage(GamePageViewModel viewModel, BLEService bluetoothLEService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        BluetoothLEService = bluetoothLEService;
        _androidDeviceOrientationService = new DeviceOrientationService();

            

        
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _androidDeviceOrientationService.SetDeviceOrientation(DisplayOrientation.Landscape);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _androidDeviceOrientationService.SetDeviceOrientation(DisplayOrientation.Portrait);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}