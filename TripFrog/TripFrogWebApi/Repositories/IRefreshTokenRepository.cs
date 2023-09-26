using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Repositories;

public interface IRefreshTokenRepository
{
    Task<IResponse<string>> GetRefreshTokenForUser(Guid userId);
    Task<IResponse<ITokensDto>> CreateRefreshAndJwtTokensForUserAsync(UserDto user);
    Task DeleteRefreshTokenForUser(Guid userId);
    Task<IResponse<ITokensDto>> RegenerateJwtTokenWithRefreshTokenAsync(TokensDto tokens);
}