namespace TripFrogModels.Models;

public sealed class City
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CountryId { get; set; }
    public Country? Country { get; set; }
    public IList<Place>? Places { get; set; }
}
