namespace BLE_Hockey.Pages;

public partial class MenuPage : ContentPage
{
	public MenuPage()
	{
		InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private void Settings_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AboutPage());
    }
}