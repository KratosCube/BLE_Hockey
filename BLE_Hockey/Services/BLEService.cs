﻿namespace BLE_Hockey.Services
{

    public class BLEService
    {

        public DeviceH NewDeviceCandidateFromHomePage { get; set; } = new();
        public List<DeviceH> DeviceCandidateList { get; private set; }
        public IBluetoothLE BluetoothLE { get; private set; }
        public IAdapter Adapter { get; private set; }
        public IDevice Device { get; set; }

        public BLEService()
        {
            BluetoothLE = CrossBluetoothLE.Current;
            Adapter = CrossBluetoothLE.Current.Adapter;
            Adapter.ScanTimeout = 4000;

            Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            Adapter.DeviceConnected += Adapter_DeviceConnected;
            Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
            Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;

            BluetoothLE.StateChanged += BluetoothLE_StateChanged;
        }


        public async Task<List<DeviceH>> ScanForDevicesAsync()
        {
            DeviceCandidateList = new List<DeviceH>();

            try
            {
                IReadOnlyList<IDevice> systemDevices = Adapter.GetSystemConnectedOrPairedDevices(HockeyTargetUuids.HockeyTargetServiceUuids);
                foreach (var systemDevice in systemDevices)
                {
                    
                    DeviceH deviceCandidate = DeviceCandidateList.FirstOrDefault(d => d.Id == systemDevice.Id);
                    if (deviceCandidate == null)
                    {
                        DeviceCandidateList.Add(new DeviceH
                        {
                            Id = systemDevice.Id,
                            Name = systemDevice.Name,
                            Rssi = systemDevice.Rssi,
                        });
                        await ShowToastAsync("Najité " +/*{systemDevice.State.ToString().ToLower()}*/$"zařízení {systemDevice.Name}.");
                        //await Adapter.ConnectToDeviceAsync(systemDevice);
                        
                    }
                    
                }
                await Adapter.StartScanningForDevicesAsync(HockeyTargetUuids.HockeyTargetServiceUuids);
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to scan nearby Bluetooth LE devices: {ex.Message}.");
                await Shell.Current.DisplayAlert($"Unable to scan nearby Bluetooth LE devices", $"{ex.Message}.", "OK");
            }

            return DeviceCandidateList;
        }







        

            #region DeviceEventArgs
            private async void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
        {
            DeviceH deviceCandidate = DeviceCandidateList.FirstOrDefault(d => d.Id == e.Device.Id);
            if (deviceCandidate == null)
            {
                DeviceCandidateList.Add(new DeviceH
                {
                    Id = e.Device.Id,
                    Name = e.Device.Name,
                });
                await ShowToastAsync($"Found {e.Device.State.ToString().ToLower()} {e.Device.Name}.");
            }
        }

        private void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await ShowToastAsync($"{e.Device.Name} connection is lost.");
                    e.Device.Dispose();
                    
                }
                catch
                {
                    await ShowToastAsync($"Device connection is lost.");
                }
            });
        }

        private void Adapter_DeviceConnected(object sender, DeviceEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    Vibration.Default.Cancel();
                    TimeSpan vibrationLength = TimeSpan.FromSeconds(0.3);
                    await ShowToastAsync($"{e.Device.Name} is connected.");
                    Vibration.Default.Vibrate(vibrationLength);
                }
                catch
                {
                    await ShowToastAsync($"Device is connected.");
                }
            });
        }

        private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    Vibration.Default.Cancel();
                    TimeSpan vibrationLength = TimeSpan.FromSeconds(1);
                    await ShowToastAsync($"{e.Device.Name} is disconnected.");
                    Vibration.Default.Vibrate(vibrationLength);
                }
                catch
                {
                    await ShowToastAsync($"Device is disconnected.");
                }
            });
        }
        #endregion DeviceEventArgs


        #region BluetoothStateChangedArgs
        private void BluetoothLE_StateChanged(object sender, BluetoothStateChangedArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await ShowToastAsync($"Bluetooth state is {e.NewState}.");
                }
                catch
                {
                    await ShowToastAsync($"Bluetooth state has changed.");
                }
            });
        }
        #endregion BluetoothStateChangedArgs


#if ANDROID
        #region BluetoothPermissions
        public async Task<PermissionStatus> CheckBluetoothPermissions()
        {
            PermissionStatus status = PermissionStatus.Unknown;
            try
            {
                status = await Permissions.CheckStatusAsync<BluetoothLEPermissions>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to check Bluetooth LE permissions: {ex.Message}.");
                await Shell.Current.DisplayAlert($"Unable to check Bluetooth LE permissions", $"{ex.Message}.", "OK");
            }
            return status;
        }

        public async Task<PermissionStatus> RequestBluetoothPermissions()
        {
            PermissionStatus status = PermissionStatus.Unknown;
            try
            {
                status = await Permissions.RequestAsync<BluetoothLEPermissions>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to request Bluetooth LE permissions: {ex.Message}.");
                await Shell.Current.DisplayAlert($"Unable to request Bluetooth LE permissions", $"{ex.Message}.", "OK");
            }
            return status;
        }
        #endregion BluetoothPermissions
#elif IOS    
#elif WINDOWS        
#endif
        public async Task ShowToastAsync(string message)
        {
            ToastDuration toastDuration = ToastDuration.Long;
            IToast toast = Toast.Make(message, toastDuration);
            await toast.Show();
        }
    }






    
}
