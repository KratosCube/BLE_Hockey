using BLE_Hockey.Services;
using Microsoft.Maui.Handlers;
using System.Text;
using System.Threading.Tasks;
using Thread = System.Threading.Thread;

namespace BLE_Hockey.ViewModels;

public partial class GamePageViewModel : BaseViewModel
{
    ConnectPageViewModel cpw;
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

    async Task UTF8DataAsync()
    {
        //if (BleService.Device != null)
        //{
        //    if (BleService.Device.State == DeviceState.Connected)
        //    {
        //        ButtonPressedService = await BleService.Device.GetServiceAsync(HockeyTargetUuids.HockeyTargetServiceUuid);
        //        if (ButtonPressedService != null)
        //        {
        //            ButtonPressedCharacteristic = await ButtonPressedService.GetCharacteristicAsync(HockeyTargetUuids.HockeyTargetCharacteristicUuid);
        //            if (ButtonPressedCharacteristic != null)
        //            {
        //                ButtonPressedCharacteristic.ValueUpdated += DeviceReadDataUtf8Async;
        //                await ButtonPressedCharacteristic.StartUpdatesAsync();

        //                //await Task.Run(async () =>
        //                //{
        //                //    await Game();
        //                //});
        //            }
        //        }
        //    }
        //}
        cpw.UTF8DataAsync();
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
    int state = 0;
    int startTime;
    int endTime;
    bool gameIsRunning = false;
    int target;
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
                        endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-2;
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
                            endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-1;
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
                            endTime = Convert.ToInt32(GetTimestamp(DateTime.Now))-1;
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

