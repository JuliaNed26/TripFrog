namespace TripFrogModels.Models;

public sealed class Place
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double? Price { get; set; }
    public string? MapsLink { get; set; }
    public Guid CityId { get; set; }
    public City? City { get; set; }
    public IList<PlacePhoto>? PlacePhotographs { get; set; }
    public IList<Destination>? Destinations { get; set; }
}
