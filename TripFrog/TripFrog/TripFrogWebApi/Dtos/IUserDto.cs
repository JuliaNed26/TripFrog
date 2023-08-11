using TripFrogModels;

namespace TripFrogWebApi.Dtos;

public interface IUserDto
{
    public Guid Id { get; }
    public string FirstName { get;}
    public string? LastName { get; }
    public string Email { get; }
    public string? Phone { get; }
    public string? PictureUrl { get; }
    public string Login { get; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public Role Role { get; }
}
