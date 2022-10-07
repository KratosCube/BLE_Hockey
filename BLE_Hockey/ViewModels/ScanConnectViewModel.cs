using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE_Hockey.ViewModels
{
    public partial class ScanConnectViewModel : BaseViewModel
    {
        BLEService BluetoothLEService;

        public ObservableCollection<DeviceH> DeviceCandidates { get; } = new();

        public IAsyncRelayCommand GoToHeartRatePageAsyncCommand { get; }
        public IAsyncRelayCommand ScanNearbyDevicesAsyncCommand { get; }
        public IAsyncRelayCommand CheckBluetoothAvailabilityAsyncCommand { get; }

        public ScanConnectViewModel(BLEService bluetoothLEService)
        {
            Title = $"Scan and select device";

            BluetoothLEService = bluetoothLEService;

            GoToHeartRatePageAsyncCommand = new AsyncRelayCommand<DeviceH>(async (devicecandidate) => await GoToHeartRatePageAsync(devicecandidate));

            ScanNearbyDevicesAsyncCommand = new AsyncRelayCommand(ScanDevicesAsync);
            CheckBluetoothAvailabilityAsyncCommand = new AsyncRelayCommand(CheckBluetoothAvailabilityAsync);
        }

        async Task GoToHeartRatePageAsync(DeviceH deviceCandidate)
        {
            if (IsScanning)
            {
                await BluetoothLEService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
                return;
            }

            if (deviceCandidate == null)
            {
                return;
            }

            BluetoothLEService.NewDeviceCandidateFromHomePage = deviceCandidate;

            Title = $"{deviceCandidate.Name}";

            await Shell.Current.GoToAsync("//HeartRatePage", true);
        }

        async Task ScanDevicesAsync()
        {
            if (IsScanning)
            {
                return;
            }

            if (!BluetoothLEService.BluetoothLE.IsAvailable)
            {
                Debug.WriteLine($"Bluetooth is missing.");
                await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
                return;
            }

#if ANDROID
            PermissionStatus permissionStatus = await BluetoothLEService.CheckBluetoothPermissions();
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await BluetoothLEService.RequestBluetoothPermissions();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert($"Bluetooth LE permissions", $"Bluetooth LE permissions are not granted.", "OK");
                    return;
                }
            }
#elif IOS
#elif WINDOWS
#endif

            try
            {
                if (!BluetoothLEService.BluetoothLE.IsOn)
                {
                    await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                    return;
                }

                IsScanning = true;

                List<DeviceH> deviceCandidates = await BluetoothLEService.ScanForDevicesAsync();

                if (deviceCandidates.Count == 0)
                {
                    await BluetoothLEService.ShowToastAsync($"Unable to find nearby Bluetooth LE devices. Try again.");
                }

                if (DeviceCandidates.Count > 0)
                {
                    DeviceCandidates.Clear();
                }

                foreach (var deviceCandidate in deviceCandidates)
                {
                    DeviceCandidates.Add(deviceCandidate);
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
        }

        async Task CheckBluetoothAvailabilityAsync()
        {
            if (IsScanning)
            {
                return;
            }

            try
            {
                if (!BluetoothLEService.BluetoothLE.IsAvailable)
                {
                    Debug.WriteLine($"Error: Bluetooth is missing.");
                    await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
                    return;
                }

                if (BluetoothLEService.BluetoothLE.IsOn)
                {
                    await Shell.Current.DisplayAlert($"Bluetooth is on", $"You are good to go.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to check Bluetooth availability: {ex.Message}");
                await Shell.Current.DisplayAlert($"Unable to check Bluetooth availability", $"{ex.Message}.", "OK");
            }
        }
    }
}
