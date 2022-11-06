namespace BLE_Hockey;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder.UseMauiApp<App>().UseMauiCommunityToolkit();

		builder
			.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});


        builder.Services.AddSingleton<BLEService>();

        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);

        builder.Services.AddSingleton<TargetFinderPageViewModel>();
        builder.Services.AddSingleton<TargetFinderPage>();

        builder.Services.AddSingleton<ConnectPageViewModel>();
        builder.Services.AddSingleton<ConnectPage>();

        builder.Services.AddSingleton<MenuPageViewModel>();
        builder.Services.AddSingleton<MenuPage>();

        builder.Services.AddSingleton<GamePageViewModel>();
        builder.Services.AddSingleton<GamePage>();

        return builder.Build();
	}
}
