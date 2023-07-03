using System.ComponentModel.DataAnnotations;

namespace TripFrog.Models;
public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }

    [MinLength(6)]
    [MaxLength(30)]
    [RegularExpression(@"^[a-zA-Z0-9]+([.]?[a-zA-Z0-9]+)*@[a-zA-Z0-9]+(\.[a-zA-Z0-9]{2,3})+$")]
    public string Email { get; set; }

    [RegularExpression(@"^\+\d{1,3}\s?\(?\d{1,5}\)?\s?\d+\s?(\-?\d+\s?)*$")]
    public string? Phone { get; set; }
    public string? PictureUrl { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public IList<LanguageUser> LanguageUsers { get; set; }
    public IList<Trip> Trips { get; set; }
    public IList<Apartment> Apartments { get; set; }
}
