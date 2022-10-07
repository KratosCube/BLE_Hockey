namespace BLE_Hockey;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(ScanConnectPage), typeof(ScanConnectPage));
    }
}
