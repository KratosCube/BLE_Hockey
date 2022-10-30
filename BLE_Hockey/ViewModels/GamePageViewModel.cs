using BLE_Hockey.Services;
using Microsoft.Maui.Handlers;
using System.Text;
using System.Threading.Tasks;

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

    int loopCounter = 0;

    [ObservableProperty]
    int hitCounter = 0;

    bool gameState = false;

    [ObservableProperty]
    string lastByteValue;

    [ObservableProperty]
    string gameOutput;

    [ObservableProperty]
    string buttonPressedValue;

    [ObservableProperty]
    string writeCommand;

    async Task UTF8DataAsync()
    {
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
                        await Game();




                    }
                }
            }
        }
    }

    public static String GetTimestamp(DateTime value)
    {
        return value.ToString("ss");
    }
    private int RandomPicker()
    {
        int number = rand.Next(0, 2);
        return number;
    }
    int state = 0;
    int startTime;
    int endTime;
    bool gameIsRunning = false;
    async Task Game()
    {

        IsHitted = true;
        gameIsRunning = true;
        if (gameIsRunning)
        {
            switch (gameState)
            {
                case false:
                    //start of game
                    if (loopCounter == 0)
                    {
                        gameState = true;
                        state = RandomPicker();
                        IsHitted = false;
                        GameOutput = "Start Game";

                    }
                    //trought game
                    else if (loopCounter != 0 && loopCounter != 3)
                    {
                        gameState = true;
                        state = RandomPicker();
                        IsHitted = false;
                        GameOutput = "";
                    }
                    //end of game
                    else if (loopCounter >= 3)
                    {
                        if (hitCounter >= 3)
                        {
                            GameOutput = "Won";
                        }
                        else
                        {
                            GameOutput = "Lose";
                        }
                        await ButtonPressedCharacteristic.StopUpdatesAsync();
                        HitCounter = 0;
                        loopCounter = 0;
                        gameState = false;
                        gameIsRunning = false;
                        return;
                    }
                    break;

                case true:
                    if (state == 1)
                    {
                        if (IsHitted == true)
                        {
                        }
                            startTime = Convert.ToInt32(GetTimestamp(DateTime.Now));
                        endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-5;
                        while (startTime > endTime)
                        {
                            IsHitted = true;
                            if (ButtonPressedValue == "AA")
                            {
                                IsHitted = false;
                                ButtonPressedValue = "";
                                break;
                            }
                            endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-5;
                        }
                        //nepovedlo se strefit terč
                        if (IsHitted == true)
                        {
                            gameState = false;
                            IsHitted = false;
                            loopCounter++;
                            state = 0;
                        }
                        //povedlo se strefit terč
                        else if(IsHitted == false)
                        {
                            gameState = false;
                            IsHitted = false;
                            HitCounter++;
                            loopCounter++;
                            state = 0;
                        }
                    }
                    else
                    {
                        startTime = Convert.ToInt32(GetTimestamp(DateTime.Now));
                        endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-5;
                        while (startTime > endTime)
                        {
                            endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-5;
                        }
                        gameState = false;
                        IsHitted = false;
                        ButtonPressedValue = "";
                        state = 0;
                    }
                    break;
            }
        }
    }
    private async void DeviceReadDataUtf8Async(object sender, CharacteristicUpdatedEventArgs e)
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

