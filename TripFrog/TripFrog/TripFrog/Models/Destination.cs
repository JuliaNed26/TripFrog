namespace TripFrog.Models;

public class Destination
{
    public Guid Id { get; set; }
    public DateTime VisitDate { get; set; }
    public bool IsVisited { get; set; }
    public Guid TripId { get; set; }
    public Trip Trip { get; set; }
    public Guid PlaceId { get; set; }
    public Place Place { get; set; }
    public IList<Ticket> Tickets { get; set; }
}
