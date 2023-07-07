namespace TripFrogModels.Models;

public sealed class Trip
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool Finished { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public IList<Booking>? Bookings { get; set; }
    public IList<Destination>? Destinations { get; set; }
}
