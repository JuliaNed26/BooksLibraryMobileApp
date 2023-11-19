using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MAUISql;

public partial class MapPage : ContentPage
{
	public MapPage(string address)
	{
		InitializeComponent();
		Address = address;
	}

	public string Address { get; init; }

	protected override async void OnAppearing()
    {
        // find the coordinates of the publishing address using geocoding
        var locations = await Geocoding.Default.GetLocationsAsync(Address);
        var publishingLocation = locations.FirstOrDefault();

        // get the user's current location
        var geolocationRequest = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(20));
        var currentLocation = await Geolocation.GetLocationAsync(geolocationRequest);

        Map.MoveToRegion(MapSpan.FromCenterAndRadius(currentLocation!, Distance.FromMiles(10)));

        var homePin = new Pin
        {
	        Address = "home address",
	        Location = currentLocation,
	        Type = PinType.Place,
	        Label = "Home"
        };

        var publishingPin = new Pin
        {
	        Address = "publishing address",
	        Location = publishingLocation!,
	        Type = PinType.Place,
	        Label = "Publishing"
        };

        Map.Pins.Add(homePin);
        Map.Pins.Add(publishingPin);

        var polyline = new Polyline
        {
	        StrokeColor = Colors.Violet,
	        StrokeWidth = 15,
	        Geopath = { currentLocation, publishingLocation }
        };
        Map.MapElements.Add(polyline);

		base.OnAppearing();
	}
}