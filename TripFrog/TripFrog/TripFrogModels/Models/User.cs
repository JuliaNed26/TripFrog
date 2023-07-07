using System.ComponentModel.DataAnnotations;

namespace TripFrogModels.Models;
public sealed class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string? Phone { get; set; }
    public string? PictureUrl { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public Role Role { get; set; } = Role.Traveler;
    public IList<LanguageUser>? LanguageUsers { get; set; }
    public IList<Trip>? Trips { get; set; }
    public IList<Apartment>? Apartments { get; set; }
}
