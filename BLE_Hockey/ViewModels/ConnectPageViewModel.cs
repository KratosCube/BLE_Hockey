using System.Text;
namespace BLE_Hockey.ViewModels;

public partial class ConnectPageViewModel : BaseViewModel
{

    public BLEService BleService { get; private set; }

    public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
    public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }
    public IAsyncRelayCommand ReadUTF8DataAsyncCommand { get; }
    public IAsyncRelayCommand WriteDataAsyncCommand { get; }
    public IService ButtonPressedService { get; private set; }
    public ICharacteristic ButtonPressedCharacteristic { get; private set; }
    public ConnectPageViewModel(BLEService bluetoothLEService)
    {
        Title = $"Connect Page";

        BleService = bluetoothLEService;

        ConnectToDeviceCandidateAsyncCommand = new AsyncRelayCommand(ConnectToDeviceCandidateAsync);

        DisconnectFromDeviceAsyncCommand = new AsyncRelayCommand(DisconnectFromDeviceAsync);

        ReadUTF8DataAsyncCommand = new AsyncRelayCommand(UTF8DataAsync);

        WriteDataAsyncCommand = new AsyncRelayCommand(DeviceWriteDataAsync);
    }

    [ObservableProperty]
    string buttonPressedValue;

    [ObservableProperty]
    string writeCommand;

    async Task UTF8DataAsync()
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
        string text = utf8.GetString(bytes, 0, bytes.Length);

        //splitting text
        string[] separatingStrings = { "[", "]" };
        string[] words = text.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
        //ButtonPressedValue = words[2];
        char lastCharacter = words[2][words[2].Length-1];
        ButtonPressedValue = Convert.ToString(lastCharacter);

        if(ButtonPressedValue == "1")
        {
            IsHitted = true;
        }
        else
        {
            IsHitted = false;
        }
    }

    async Task DeviceWriteDataAsync()
    {
        // LedR On [0L0211]
        byte[] bytes = Encoding.ASCII.GetBytes(writeCommand);
        ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
        if (ButtonPressedService != null)
        {
            ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
            if (ButtonPressedCharacteristic != null)
            {
                await ButtonPressedCharacteristic.WriteAsync(bytes);
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
                            await DisconnectFromDeviceAsync();
                            await BleService.ShowToastAsync($"{BleService.Device.Name} has been disconnected.");
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
            ButtonPressedValue = "0l";
            BleService.Device?.Dispose();
            BleService.Device = null;
            await Shell.Current.GoToAsync("//TargetFinderPage", true);
        }
    }


}