using System.ComponentModel.DataAnnotations;
using TripFrogModels;

namespace TripFrogWebApi.DTO;

public interface IChangedUserInfoDto
{
    public Guid Id { get; }
    public string FirstName { get; }
    public string? LastName { get; }

    [EmailAddress]
    public string Email { get; }

    [Phone]
    public string? Phone { get; }
    public string? PictureUrl { get; }

    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,13}$",
        ErrorMessage = "Password length should be 8-13 symbols with numbers and letters")]
    public string? Password { get; }
    public Role Role { get; }
}
