using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;
using TripFrogWebApi.TokensCreator;

namespace TripFrogWebApi.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly TripFrogContext _context;
    private readonly IJwtTokenService _tokensService;

    public RefreshTokenRepository(TripFrogContext dbContext, IJwtTokenService tokensService)
    {
        _context = dbContext;
        _tokensService = tokensService;
    }

    public async Task<Tokens> CreateNewTokensForUserAsync(IUserDto user)
    {
        var newJwtToken = _tokensService.GenerateJwtToken(user);

        return new Tokens
        {
            JwtToken = newJwtToken,
            RefreshToken = await GenerateNewRefreshToken(user.Id),
        };
    }

    public async Task<(bool successful, string message)> TryValidateRefreshTokenAsync(string token)
    {
        var refreshToken =
            await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token.ToString() == token);

        if (refreshToken == null)
        {
            return (false, "Refresh token does not exist");
        }

        if (refreshToken.ExpirationDate < DateTime.UtcNow)
        {
            return (false, "Refresh token has expired, log in again");
        }

        return (true, string.Empty);
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

    private async Task<string> GenerateNewRefreshToken(Guid userId)
    {
        var refreshTokenEntity = new RefreshToken
        {
            Token = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            UserId = userId,
            User = await _context.Users.SingleAsync(user => user.Id == userId),
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return refreshTokenEntity.Token.ToString();
    }
}
