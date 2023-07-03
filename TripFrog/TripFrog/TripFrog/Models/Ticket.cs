namespace TripFrog.Models;

public class Ticket
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FileUrl { get; set; }
    public Guid DestinationId { get; set; }
    public Destination Destination { get; set; }
}
