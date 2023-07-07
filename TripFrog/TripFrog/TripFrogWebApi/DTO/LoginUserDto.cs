using System.ComponentModel.DataAnnotations;

namespace TripFrogWebApi.DTO;

public sealed class LoginUserDto
{
    [EmailAddress]
    public string Email { get; set; }
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,13}$")]
    public string Password { get; set; }
}
