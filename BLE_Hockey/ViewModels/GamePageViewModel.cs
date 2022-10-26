using BLE_Hockey.Services;
using System.Text;
using System.Threading.Tasks;

namespace BLE_Hockey.ViewModels;

public partial class GamePageViewModel : BaseViewModel
{
    public BLEService BleService { get; private set; }
    public IAsyncRelayCommand ReadUTF8DataAsyncCommand { get; }
    public IAsyncRelayCommand WriteDataAsyncCommand { get; }
    public IService ButtonPressedService { get; private set; }
    public ICharacteristic ButtonPressedCharacteristic { get; private set; }
    Random rand = new Random();

    public GamePageViewModel(BLEService bluetoothLEService)
    {
        BleService = bluetoothLEService;

        ReadUTF8DataAsyncCommand = new AsyncRelayCommand(UTF8DataAsync);

        WriteDataAsyncCommand = new AsyncRelayCommand(DeviceWriteDataAsync);
    }

    int loopCounter = 0;


    int randPickNum = 0;

    [ObservableProperty]
    int hitCounter = 1;

    bool gameState = false;

    [ObservableProperty]
    string lastByteValue;

    [ObservableProperty]
    string firstByteValue;

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
                    }
                }
            }
        }
    }


    private int RandomPicker()
    {
        Task.Delay(1000);
        int number = rand.Next(0, 101);
        return number;
    }

    private async void Game()
    {

        //if 


        if (randPickNum > 80 || gameState)
        {
            GameOutput = "";
            if (FirstByteValue == "A" && gameState == false)
            {

                gameState = true;
                IsHitted = true;

            }
            if (LastByteValue == "1")
            {
                IsHitted = false;

            }
        }
        if (FirstByteValue == "0" && gameState)
        {

            if (IsHitted == false)
            {
                HitCounter++;
                IsHitted = false;
                gameState = false;
            }
            else
            {
                IsHitted = false;
                gameState = false;
            }
            loopCounter++;
        }
        if (loopCounter == 3)
        {
            if (hitCounter == 3)
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

