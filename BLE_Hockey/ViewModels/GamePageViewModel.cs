using BLE_Hockey.Services;
using System.Text;

namespace BLE_Hockey.ViewModels;

public partial class GamePageViewModel : BaseViewModel
{
    public BLEService BleService { get; private set; }
    public IAsyncRelayCommand ReadUTF8DataAsyncCommand { get; }
    public IAsyncRelayCommand WriteDataAsyncCommand { get; }
    public IService ButtonPressedService { get; private set; }
    public ICharacteristic ButtonPressedCharacteristic { get; private set; }


    public GamePageViewModel(BLEService bluetoothLEService)
    {
        BleService = bluetoothLEService;

        ReadUTF8DataAsyncCommand = new AsyncRelayCommand(UTF8DataAsync);

        WriteDataAsyncCommand = new AsyncRelayCommand(DeviceWriteDataAsync);
    }

    int loopCounter = 0;
    int hitCounter = 0;

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
        LastByteValue = Convert.ToString(lastCharacter);

        char firstCharacter = words[2][words[2].Length-6];
        FirstByteValue = Convert.ToString(firstCharacter);

        if (FirstByteValue == "A" || gameState)
        {
            GameOutput = "Won";
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
                hitCounter++;
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
        if(loopCounter >= 3)
        {
            if(hitCounter >= 3)
            {
                GameOutput = "Won";
            }
            else
            {
                GameOutput = "Lose";
            }
            ButtonPressedCharacteristic.StopUpdatesAsync();
            loopCounter = 0;
            hitCounter = 0;
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
}

