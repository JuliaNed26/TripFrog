using TripFrogModels;

namespace TripFrogWebApi.DTO;

public interface IUserDto
{
    public Guid Id { get; }
    public string FirstName { get;}
    public string? LastName { get; }
    public string Email { get; }
    public string? Phone { get; }
    public string? PictureUrl { get; }
    public Role Role { get; }
}
