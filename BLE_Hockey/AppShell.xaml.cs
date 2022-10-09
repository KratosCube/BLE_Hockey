namespace BLE_Hockey;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(ConnectPage), typeof(ConnectPage));
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
    }
}
