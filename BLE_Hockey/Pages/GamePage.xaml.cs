namespace BLE_Hockey.Pages;

public partial class GamePage : ContentPage
{
    public BLEService BluetoothLEService { get; private set; }
    public GamePage(GamePageViewModel viewModel, BLEService bluetoothLEService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        BluetoothLEService = bluetoothLEService;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
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