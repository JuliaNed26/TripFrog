using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Services;

namespace TripFrogWebApi.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly TripFrogContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenRepository(TripFrogContext dbContext, IJwtTokenService jwtTokenService)
    {
        _context = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<IResponse<string>> GetRefreshTokenForUser(Guid userId)
    {
        var foundRefreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.UserId == userId);
        if (foundRefreshToken == null)
        {
            return new Response<string>
            {
                Successful = false,
                Message = "Refresh token for user was not found"
            };
        }

        return new Response<string>
        {
            Successful = true,
            Data = foundRefreshToken.Token.ToString()
        };
    }

    public async Task<IResponse<ITokensDto>> CreateRefreshAndJwtTokensForUserAsync(UserDto user)
    {
        var foundUserById = await _context.Users.SingleOrDefaultAsync(x => x.Id == user.Id);

        if (foundUserById is null)
        {
            return new Response<ITokensDto>
            {
                Successful = false,
                Message = "Can not find User with such id"
            };
        }

        var newJwtToken = _jwtTokenService.GenerateJwtToken(user);
        var newRefreshToken = await GenerateNewRefreshTokenForUserAsync(foundUserById);

        return new Response<ITokensDto>
        {
            Successful = true,
            Data = new TokensDto
            {
                JwtToken = (JwtTokenDto)newJwtToken,
                RefreshToken = newRefreshToken
            }
        };
    }

    public async Task DeleteRefreshTokenForUser(Guid userId)
    {
        var refreshTokenByUserId = await _context.RefreshTokens.SingleOrDefaultAsync(token => token.UserId == userId);

        if (refreshTokenByUserId != null)
        {
            _context.RefreshTokens.Remove(refreshTokenByUserId);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IResponse<ITokensDto>> RegenerateJwtTokenWithRefreshTokenAsync(TokensDto tokens)
    {
        var jwtTokenValidationResult = ValidateJwtToken(tokens.JwtToken.Token);
        var refreshTokenValidationResult = await ValidateRefreshTokenAsync(tokens.RefreshToken);

        var validationSuccess = jwtTokenValidationResult.successful && refreshTokenValidationResult.successful;

        if (!validationSuccess)
        {
            var errorMessage = !jwtTokenValidationResult.successful
                                     ? jwtTokenValidationResult.errorMessage
                                     : refreshTokenValidationResult.errorMessage; 

            return new Response<ITokensDto>
            {
                Successful = false,
                Message = errorMessage
            };
        }

        _jwtTokenService.TryGetUserInfoFromToken(tokens.JwtToken.Token, out IUserDto userFromOldJwtToken);
        var newJwtToken = _jwtTokenService.GenerateJwtToken(userFromOldJwtToken);

        return new Response<ITokensDto>
        {
            Successful = true,
            Data = new TokensDto
            {
                JwtToken = (JwtTokenDto)newJwtToken,
                RefreshToken = tokens.RefreshToken,
            }
        };
    }

    private async Task<string> GenerateNewRefreshTokenForUserAsync(User user)
    {
        var refreshTokenEntity = new RefreshToken
        {
            Token = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            UserId = user.Id,
            User = user
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return refreshTokenEntity.Token.ToString();
    }

    private (bool successful, string errorMessage) ValidateJwtToken(string token)
    {
        if (!_jwtTokenService.TryGetClaimsFromToken(token, out var tokenClaimsPrincipal))
        {
            return (false, "Can not get claims from jwt token");
        }

        var jwtExpirationDate = _jwtTokenService.GetTokenExpirationDate(tokenClaimsPrincipal);

        if (jwtExpirationDate > DateTime.UtcNow)
        {
            return (false, "Jwt token has not expired yet");
        }

        _jwtTokenService.TryGetUserInfoFromToken(token, out IUserDto userFromJwtToken);

        if (!_context.Users.Any(x => x.Id == userFromJwtToken.Id))
        {
            return (false, "User with such jwt token does not exist");
        }

        return (true, string.Empty);
    }

    private async Task<(bool successful, string errorMessage)> ValidateRefreshTokenAsync(string token)
    {
        var foundRefreshToken = await _context.RefreshTokens
                                              .FirstOrDefaultAsync(x => x.Token.ToString() == token);

        if (foundRefreshToken == null)
        {
            return (false, "Refresh token does not exist");
        }

        if (foundRefreshToken.ExpirationDate < DateTime.UtcNow)
        {
            return (false, "Refresh token has expired, log in again");
        }

        return (true, string.Empty);
    }
}
