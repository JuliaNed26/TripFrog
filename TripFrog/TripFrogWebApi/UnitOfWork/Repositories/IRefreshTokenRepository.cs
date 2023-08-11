using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Repositories;

public interface IRefreshTokenRepository
{
    Task<Tokens> CreateNewTokensForUserAsync(IUserDto user);
    Task<(bool successful, string message)> TryValidateRefreshTokenAsync(string token);
    Task DeleteRefreshTokenForUser(Guid userId);
}