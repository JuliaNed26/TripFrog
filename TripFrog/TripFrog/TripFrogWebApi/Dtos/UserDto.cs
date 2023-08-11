using TripFrogModels;

namespace TripFrogWebApi.Dtos;

public sealed class UserDto : IUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? PictureUrl { get; set; }
    public string Login { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public Role Role { get; set; }
}
