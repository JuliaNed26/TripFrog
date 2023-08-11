using System.ComponentModel.DataAnnotations;
using TripFrogModels;

namespace TripFrogWebApi.Dtos;

public sealed class RegisterUserDto : IRegisterUserDto
{
    public string FirstName { get; set; }
    public string? LastName { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string? Phone { get; set; }
    public string? PictureUrl { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
}
