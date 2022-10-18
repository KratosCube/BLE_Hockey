

namespace BLE_Hockey.Pages;

public partial class ConnectPage : ContentPage
{
    public BLEService BluetoothLEService { get; private set; }

    public IService ButtonPressedService { get; private set; }

    public ICharacteristic ButtonPressedCharacteristic { get; private set; }

    public ConnectPage(ConnectPageViewModel viewModel, BLEService bluetoothLEService)
	{
		InitializeComponent();
        BindingContext = viewModel;
        BluetoothLEService = bluetoothLEService;
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
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