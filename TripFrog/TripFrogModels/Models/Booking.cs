namespace TripFrogModels.Models;

public sealed class Booking
{
    public Guid Id { get; set; }
    public DateTime StartVisitDate { get; set; }
    public DateTime EndVisitDate { get; set; }
    public Guid TripId { get; set; }
    public Trip Trip { get; set; }
    public Guid ApartmentId { get; set; }
    public Apartment? Apartment { get; set; }
}
