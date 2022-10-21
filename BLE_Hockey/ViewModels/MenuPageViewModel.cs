namespace BLE_Hockey.ViewModels;

public class MenuPageViewModel : BaseViewModel
{
    BLEService BluetoothLEService;
    public IAsyncRelayCommand RouteTargetFinderPageAsyncCommand { get; }
    public IAsyncRelayCommand RouteConnectPageAsyncCommand { get; }
    public IAsyncRelayCommand RouteGamePageAsyncCommand { get; }

    public MenuPageViewModel(BLEService bluetoothLEService)
	{
        BluetoothLEService = bluetoothLEService;

        RouteTargetFinderPageAsyncCommand = new AsyncRelayCommand<DeviceH>(async (devicecandidate) => await RouteTargetFinderPageAsync(devicecandidate));

        RouteConnectPageAsyncCommand = new AsyncRelayCommand<DeviceH>(async (devicecandidate) => await RouteConnectPageAsync(devicecandidate));

        RouteGamePageAsyncCommand = new AsyncRelayCommand<DeviceH>(async (devicecandidate) => await RouteGamePageAsync(devicecandidate));

    }

    async Task RouteTargetFinderPageAsync(DeviceH deviceCandidate)
    {

        await Shell.Current.GoToAsync("//TargetFinderPage", true);
    }


    async Task RouteConnectPageAsync(DeviceH deviceCandidate)
    {


        await Shell.Current.GoToAsync("//ConnectPage", true);
    }

    async Task RouteGamePageAsync(DeviceH deviceCandidate)
    {


        await Shell.Current.GoToAsync("//GamePage", true);
    }

}