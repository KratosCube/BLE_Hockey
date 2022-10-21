namespace BLE_Hockey;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(TargetFinderPage), typeof(TargetFinderPage));
        Routing.RegisterRoute(nameof(ConnectPage), typeof(ConnectPage));
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
    }
}
