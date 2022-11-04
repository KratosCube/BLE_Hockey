using BLE_Hockey.Services;
using Microsoft.Maui.Handlers;
using System.Text;
using System.Threading.Tasks;
using Thread = System.Threading.Thread;

namespace BLE_Hockey.ViewModels;

public partial class GamePageViewModel : BaseViewModel
{
    public BLEService BleService { get; private set; }
    public IAsyncRelayCommand StartGameAsyncCommand { get; }
    public IAsyncRelayCommand WriteDataAsyncCommand { get; }
    public IService ButtonPressedService { get; private set; }
    public ICharacteristic ButtonPressedCharacteristic { get; private set; }
    Random rand = new Random();

    public GamePageViewModel(BLEService bluetoothLEService)
    {
        BleService = bluetoothLEService;

        StartGameAsyncCommand = new AsyncRelayCommand(UTF8DataAsync);

        WriteDataAsyncCommand = new AsyncRelayCommand(DeviceWriteDataAsync);


    }

    public ObservableCollection<DeviceH> FoundDevices { get; } = new();

    [ObservableProperty]
    int hitCounter = 0;

    bool gameState = false;

    [ObservableProperty]
    int loopCounter;

    [ObservableProperty]
    string gameOutput;

    [ObservableProperty]
    string buttonPressedValue;

    [ObservableProperty]
    string writeCommand;


    int state = 0;
    int startTime;
    int endTime;
    bool gameIsRunning = false;
    int target;


    async Task UTF8DataAsync()
    {
        try
        {
            IsScanning = true;
            List<DeviceH> deviceCandidates = await BleService.ScanForDevicesAsync();

            if (deviceCandidates.Count == 0)
            {
                await BleService.ShowToastAsync($"Unable to find nearby Bluetooth LE devices. Try again.");
            }

            if (FoundDevices.Count > 0)
            {
                FoundDevices.Clear();
            }

            foreach (var deviceCandidate in deviceCandidates)
            {
                FoundDevices.Add(deviceCandidate);
                BleService.NewDeviceCandidateFromHomePage = deviceCandidate;
                await ConnectToDeviceCandidateAsync();

                if (BleService.Device != null)
                {
                    if (BleService.Device.State == DeviceState.Connected)
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
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get nearby Bluetooth LE devices: {ex.Message}");
            await Shell.Current.DisplayAlert($"Unable to get nearby Bluetooth LE devices", $"{ex.Message}.", "OK");
        }
        finally
        {
            IsScanning = false;
        }
        if (BleService.Device.State == DeviceState.Connected)
        {
            await Task.Run(async () =>
        {
            await Task.Delay(1000);
            await Game();
        });
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
        }
        finally
        {
            IsBusy = false;
        }
    }
    public static System.String GetTimestamp(DateTime value)
    {
        return value.ToString("ss");
    }
    private int RandomPicker(string state)
    {
        int number = 0;
        if (state == "timerDelay")
        {
            number = rand.Next(1000, 3000);
        }
        if (state == "target")
        {
            number = rand.Next(1, 3);
        }

        return number;
    }
    async Task Game()
    {
        await ButtonPressedCharacteristic.StartUpdatesAsync();
        IsHitted = false;
        IsHitted1 = false;
        gameIsRunning = true;
        HitCounter = 0;
        LoopCounter = 0;
        while (gameIsRunning)
        {
            switch (gameState)
            {
                case false:


                    //start of game
                    if (LoopCounter == 0)
                    {
                        gameState = true;
                        state = RandomPicker("timerDelay");
                        IsHitted = false;
                        IsHitted1 = false;
                        GameOutput = "Start Game";

                    }
                    //trought game
                    else if (LoopCounter != 0 && LoopCounter != 5)
                    {
                        gameState = true;
                        state = RandomPicker("timerDelay");
                        IsHitted = false;
                        IsHitted1 = false;
                        GameOutput = "";
                    }
                    //end of game
                    else if (LoopCounter >= 5)
                    {
                        if (hitCounter >= 3)
                        {
                            GameOutput = "Won";
                        }
                        else
                        {
                            GameOutput = "Lose";
                        }

                        gameState = false;
                        gameIsRunning = false;
                        return;
                    }
                    break;



                case true:

                    target = RandomPicker("target");
                    await Task.Delay(state);
                    if (target == 1)
                    {
                        startTime = Convert.ToInt32(GetTimestamp(DateTime.Now));
                        endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-3;
                        ButtonPressedValue = "";
                        while (startTime > endTime)
                        {
                            await ButtonPressedCharacteristic.StartUpdatesAsync();
                            IsHitted = true;
                            IsHitted1 = false;
                            if (ButtonPressedValue == "AA")
                            {
                                IsHitted = false;
                                IsHitted1 = false;
                                ButtonPressedValue = "";
                                break;
                            }
                            endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-3;
                        }
                        //nepovedlo se strefit terč
                        if (IsHitted == true)
                        {
                            gameState = false;
                            IsHitted = false;
                            IsHitted1 = false;
                            LoopCounter++;

                            state = 0;
                        }
                        //povedlo se strefit terč
                        else if (IsHitted == false)
                        {
                            gameState = false;
                            IsHitted = false;
                            IsHitted1 = false;
                            HitCounter++;
                            LoopCounter++;
                            state = 0;
                        }
                    }

                    else if (target == 2)
                    {
                        startTime = Convert.ToInt32(GetTimestamp(DateTime.Now));
                        endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-2;
                        ButtonPressedValue = "";
                        while (startTime > endTime)
                        {
                            await ButtonPressedCharacteristic.StartUpdatesAsync();
                            IsHitted = false;
                            IsHitted1 = true;
                            if (ButtonPressedValue == "BB")
                            {
                                IsHitted = false;
                                IsHitted1 = false;
                                ButtonPressedValue = "";
                                break;
                            }
                            endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-3;
                        }
                        //nepovedlo se strefit terč
                        if (IsHitted1 == true)
                        {
                            gameState = false;
                            IsHitted = false;
                            IsHitted1 = false;
                            LoopCounter++;

                            state = 0;
                        }
                        //povedlo se strefit terč
                        else if (IsHitted1 == false)
                        {
                            gameState = false;
                            IsHitted = false;
                            IsHitted1 = false;
                            HitCounter++;
                            LoopCounter++;
                            state = 0;
                        }
                    }
                    break;
            }
        }

        await ButtonPressedCharacteristic.StopUpdatesAsync();
    }
    private void DeviceReadDataUtf8Async(object sender, CharacteristicUpdatedEventArgs e)
    {
        var utf8 = Encoding.UTF8;
        var bytes = e.Characteristic.Value;
        ButtonPressedValue = bytes[0].ToString("X");
    }

    async Task DeviceWriteDataAsync()
    {
        // LedR On [0L0211]
        if (BleService.Device != null)
        {
            if (BleService.Device.State == DeviceState.Connected)
            {
                if (writeCommand != null)
                {
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
            }
        }
    }
}

