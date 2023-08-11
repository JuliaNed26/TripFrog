using System.ComponentModel.DataAnnotations;
using TripFrogModels;

namespace TripFrogWebApi.DTO;

public sealed class UserDto : IUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string? Phone { get; set; }
    public string? PictureUrl { get; set; }
    public Role Role { get; set; }
}
