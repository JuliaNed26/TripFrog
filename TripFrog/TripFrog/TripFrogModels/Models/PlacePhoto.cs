namespace TripFrogModels.Models;

public sealed class PlacePhoto
{
    public Guid Id { get; set; }
    public string PhotoUrl { get; set; }
    public Guid PlaceId { get; set; }
    public Place? Place { get; set; }
}
