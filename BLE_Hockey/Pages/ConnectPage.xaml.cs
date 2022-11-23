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
    private bool _translated = false;
    private async void ConnectToButton_Clicked(object sender, EventArgs e)
    {
        DisconnectFromButton.IsVisible = true;
        ConnectToButton.Animate<Thickness>("btn", value =>
        {
            int factor = Convert.ToInt32(value * 10);

            var rightmargin = (factor * 10) * -1;
            return new Thickness(0, 0, rightmargin, 20);
        },
        newThickness => ConnectToButton.Margin = newThickness, length: 200);

        DisconnectFromButton.Animate<Thickness>("btn", value =>
        {
            int factor = Convert.ToInt32(value * 10);

            var rightmargin =   (factor * 10) -100;
            return new Thickness(0,0, rightmargin, 20);
        },
        newThickness => DisconnectFromButton.Margin = newThickness, length: 200);

        
    }

    private void DisconnectFromButton_Clicked(object sender, EventArgs e)
    {
        ConnectToButton.Animate<Thickness>("btn", value =>
        {
            int factor = Convert.ToInt32(value * 10);
            var rightmargin = (factor * 10) -100;
            
            return new Thickness(0, 0, rightmargin, 20);
        },
        newThickness => ConnectToButton.Margin = newThickness, length: 200);

        DisconnectFromButton.Animate<Thickness>("btn", value =>
        {
            int factor = Convert.ToInt32(value * 10);

            var rightmargin = (factor * 10) * -1;
            return new Thickness(0, 0, rightmargin, 20);
        },
        newThickness => DisconnectFromButton.Margin = newThickness, length: 200);

        DisconnectFromButton.IsVisible = false;
    }
}