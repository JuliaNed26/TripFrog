namespace TripFrog.Models;

public class Apartment
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string LocationLink { get; set; }
    public int BathroomsCount { get; set; }
    public int LivingRoomsCount { get; set; }
    public int BedroomsCount { get; set; }
    public int GuestsCount { get; set; }
    public bool ChildrenAllowed { get; set; }
    public bool PetsAllowed { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime CheckOutTime { get; set; }
    public double PricePerDay { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public IList<ApartmentPhoto> ApartmentPhotos { get; set; }
    public IList<Booking> Bookings { get; set; }
}
