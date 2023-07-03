namespace TripFrog.Models;

public class Country
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IList<City> Cities { get; set; }
}
