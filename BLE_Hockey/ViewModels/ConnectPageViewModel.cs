using CommunityToolkit.Maui.Converters;
using System.Text;
namespace BLE_Hockey.ViewModels;

public partial class ConnectPageViewModel : BaseViewModel
{

    public BLEService BleService { get; private set; }

    public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
    public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }
    public IAsyncRelayCommand ReadUTF8DataAsyncCommand { get; }
    public IAsyncRelayCommand WriteTimerDataAsyncCommand { get; }
    public IAsyncRelayCommand WriteSetIdDataAsyncCommand { get; }
    public IAsyncRelayCommand WriteSleepDataAsyncCommand { get; }
    public IAsyncRelayCommand InfoAsyncCommand { get; }
    public IService ButtonPressedService { get; private set; }
    public ICharacteristic ButtonPressedCharacteristic { get; private set; }
    public ConnectPageViewModel(BLEService bluetoothLEService)
    {

        Title = $"Connect Page";

        BleService = bluetoothLEService;

        ConnectToDeviceCandidateAsyncCommand = new AsyncRelayCommand(ConnectToDeviceCandidateAsync);

        DisconnectFromDeviceAsyncCommand = new AsyncRelayCommand(DisconnectFromDeviceAsync);

        ReadUTF8DataAsyncCommand = new AsyncRelayCommand(UTF8DataAsync);

        WriteTimerDataAsyncCommand = new AsyncRelayCommand(DeviceWriteTimeDataAsync);

        WriteSetIdDataAsyncCommand = new AsyncRelayCommand(DeviceWriteIdDataAsync);

        WriteSleepDataAsyncCommand = new AsyncRelayCommand(DeviceWriteSleepDataAsync);

        InfoAsyncCommand = new AsyncRelayCommand(ShowInfoDataAsync);

    }

    [ObservableProperty]
    string buttonPressedValue;

    [ObservableProperty]
    string writeTime;

    [ObservableProperty]
    string writeId;

    [ObservableProperty]
    Color connectedColor = Color.FromRgb(0, 0, 0);

    public async Task UTF8DataAsync()
    {
        ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
        if (ButtonPressedService != null)
        {
            ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
            if (ButtonPressedCharacteristic != null)
            {
                ButtonPressedCharacteristic.ValueUpdated += DeviceReadDataUtf8Async;
                await ButtonPressedCharacteristic.StartUpdatesAsync();
            }
        }
    }

    private void DeviceReadDataUtf8Async(object sender, CharacteristicUpdatedEventArgs e)
    {
        var utf8 = Encoding.UTF8;
        var bytes = e.Characteristic.Value;
        ButtonPressedValue = bytes[0].ToString("X");
    }

    public async Task ShowInfoDataAsync()
    {
        ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
        if (ButtonPressedService != null)
        {
            ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
            if (ButtonPressedCharacteristic != null)
            {
                try
                {
                    await Shell.Current.DisplayAlert("Info", $"MAC: {BleService.Device.NativeDevice.ToString()}", "OK");
                }
                catch (Exception e)
                {
                    ButtonPressedValue = $"Problem ";
                }

            }
        }
    }

    private async Task DeviceWriteIdDataAsync()
    {
        //01 00 0002
        byte[] bytes = StringToByteArray(writeId);
        var message = new byte[] { 0x01, bytes[0], bytes[1], bytes[2] };
        ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
        if (ButtonPressedService != null)
        {
            ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
            if (ButtonPressedCharacteristic != null)
            {
                try
                {
                    await ButtonPressedCharacteristic.WriteAsync(message);
                    ButtonPressedValue = message[0].ToString() + "-" + message[1].ToString();
                }
                catch (Exception e)
                {
                    ButtonPressedValue = $"Problem {e.Message}";
                }

            }
        }

    }

    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    private async Task DeviceWriteTimeDataAsync()
    {

        byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(writeTime));
        var message = new byte[] { 0x02, 0x01, bytes[1], bytes[0] };
        ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
        if (ButtonPressedService != null)
        {
            ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
            if (ButtonPressedCharacteristic != null)
            {
                try
                {
                    await ButtonPressedCharacteristic.WriteAsync(message);
                    ButtonPressedValue = message[2].ToString() + "-" + message[3].ToString();
                }
                catch (Exception e)
                {
                    ButtonPressedValue = $"Problem {e.Message}";
                }
            }
        }
    }
    private async Task DeviceWriteSleepDataAsync()
    {

        var message = new byte[] { 0x05, 0x01 };
        ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
        if (ButtonPressedService != null)
        {
            ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
            if (ButtonPressedCharacteristic != null)
            {
                try
                {
                    await ButtonPressedCharacteristic.WriteAsync(message);
                    ButtonPressedValue = message[0].ToString() + "-" + message[1].ToString();
                }
                catch (Exception e)
                {
                    ButtonPressedValue = $"Problem {e.Message}";
                }
            }
        }

    }

    private async Task ConnectToDeviceCandidateAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (BleService.NewDeviceCandidateFromHomePage.Id.Equals(Guid.Empty))
        {
            #region read device id from storage
            var device_name = await SecureStorage.Default.GetAsync("device_name");
            var device_id = await SecureStorage.Default.GetAsync("device_id");
            if (!string.IsNullOrEmpty(device_id))
            {
                BleService.NewDeviceCandidateFromHomePage.Name = device_name;
                BleService.NewDeviceCandidateFromHomePage.Id = Guid.Parse(device_id);
            }
            #endregion read device id from storage
            else
            {
                await BleService.ShowToastAsync($"Select a Bluetooth LE device first. Try again.");
                return;
            }
        }

        if (!BleService.BluetoothLE.IsOn)
        {
            await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
            return;
        }

        if (BleService.Adapter.IsScanning)
        {
            await BleService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
            return;
        }

        try
        {
            IsBusy = true;
            IsHitted = true;
            IsConnected = true;
            ConnectedColor = Color.FromRgb(0, 255, 0);
            if (BleService.Device != null)
            {
                if (BleService.Device.State == DeviceState.Connected)
                {
                    if (BleService.Device.Id.Equals(BleService.NewDeviceCandidateFromHomePage.Id))
                    {
                        await BleService.ShowToastAsync($"{BleService.Device.Name} is already connected.");
                        return;
                    }

                    if (BleService.NewDeviceCandidateFromHomePage != null)
                    {
                        #region another device
                        if (!BleService.Device.Id.Equals(BleService.NewDeviceCandidateFromHomePage.Id))
                        {
                            Title = $"{BleService.NewDeviceCandidateFromHomePage.Name}";
                            await BleService.ShowToastAsync($"{BleService.NewDeviceCandidateFromHomePage.Name} has been connected.");
                        }
                        #endregion another device
                    }
                }
            }

            BleService.Device = await BleService.Adapter.ConnectToKnownDeviceAsync(BleService.NewDeviceCandidateFromHomePage.Id);


            if (BleService.Device.State == DeviceState.Connected)
            {
                ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
                if (ButtonPressedService != null)
                {
                    ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
                    if (ButtonPressedCharacteristic != null)
                    {
                        if (ButtonPressedCharacteristic.CanUpdate)
                        {
                            Title = $"{BleService.Device.Name}";

                            #region save device id to storage
                            await SecureStorage.Default.SetAsync("device_name", $"{BleService.Device.Name}");
                            await SecureStorage.Default.SetAsync("device_id", $"{BleService.Device.Id}");
                            #endregion save device id to storage
                        }
                    }
                }
            }


        }

        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to connect to {BleService.NewDeviceCandidateFromHomePage.Name} {BleService.NewDeviceCandidateFromHomePage.Id}: {ex.Message}.");
            await Shell.Current.DisplayAlert($"{BleService.NewDeviceCandidateFromHomePage.Name}", $"Unable to connect to {BleService.NewDeviceCandidateFromHomePage.Name}.", "OK");
            IsConnected = false;
            ConnectedColor = Color.FromRgb(255, 0, 0);
        }
        finally
        {
            IsBusy = false;
            IsHitted = false;
        }
    }

    private async Task DisconnectFromDeviceAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (BleService.Device == null)
        {
            await BleService.ShowToastAsync($"Nothing to do.");
            return;
        }

        if (!BleService.BluetoothLE.IsOn)
        {
            await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
            return;
        }

        if (BleService.Adapter.IsScanning)
        {
            await BleService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
            return;
        }

        if (BleService.Device.State == DeviceState.Disconnected)
        {
            await BleService.ShowToastAsync($"{BleService.Device.Name} is already disconnected.");
            return;
        }

        try
        {
            IsBusy = true;
            
            await BleService.Adapter.DisconnectDeviceAsync(BleService.Device);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to disconnect from {BleService.Device.Name} {BleService.Device.Id}: {ex.Message}.");
            await Shell.Current.DisplayAlert($"{BleService.Device.Name}", $"Unable to disconnect from {BleService.Device.Name}.", "OK");
        }
        finally
        {
            IsBusy = false;
            IsConnected = false;
            BleService.Device?.Dispose();
            BleService.Device = null;
            ConnectedColor = Color.FromRgb(255, 0, 0);
            //await Shell.Current.GoToAsync("//TargetFinderPage", true);
        }
    }


}