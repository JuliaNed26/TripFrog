using TripFrogModels;

namespace TripFrogWebApi.Dtos;

public interface IRegisterUserDto
{
    public string FirstName { get; }
    public string? LastName { get; }
    public string Email { get; }
    public string? Phone { get; }
    public string Login { get; }
    public string Password { get; }
    public Role Role { get; }
}
