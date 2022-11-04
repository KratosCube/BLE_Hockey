namespace BLE_Hockey.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotHitted))]
        bool isHitted;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotHitted1))]
        bool isHitted1;


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotScanning))]
        bool isScanning;

        [ObservableProperty]
        string title;

        public bool IsNotBusy => !IsBusy;
        public bool IsNotScanning => !isScanning;

        public bool IsNotHitted => !IsHitted;

        public bool IsNotHitted1 => !IsHitted1;
    }
}
