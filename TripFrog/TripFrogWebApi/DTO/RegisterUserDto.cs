using System.ComponentModel.DataAnnotations;
using TripFrogModels;

namespace TripFrogWebApi.DTO;

public class RegisterUserDto :IRegisterUserDto
{
    public string FirstName { get; set; }
    public string? LastName { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,13}$",
                              ErrorMessage = "Password length should be 8-13 symbols with numbers and letters")]
    public string Password { get; set; }
    public Role Role { get; set; }
}
