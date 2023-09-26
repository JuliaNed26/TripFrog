using TripFrogMVC.Services;
using TripFrogMVC.Services.WebApiClients;
using TripFrogWebApi.DTO;

namespace TripFrogMVC.Middleware;

public class TokenExpirationCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MemoryCacheManagerService _cacheManager;
    private readonly RefreshTokenApiClient _refreshTokenApiClientClient;

    public TokenExpirationCheckMiddleware(RequestDelegate next, MemoryCacheManagerService cacheManager, RefreshTokenApiClient refreshTokenApiClient)
    {
        _next = next;
        _cacheManager = cacheManager;
        _refreshTokenApiClientClient = refreshTokenApiClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var jwtToken = _cacheManager.GetJwtToken();

        if (jwtToken is not null && jwtToken.ExpirationDate < DateTime.UtcNow)
        {
            var loggedUser = _cacheManager.GetLoggedUser();
            var newToken = await _refreshTokenApiClientClient.RefreshJwtTokenForUser((JwtTokenDto)jwtToken, (UserDto)loggedUser);
            _cacheManager.SetJwtToken(newToken);
        }

        await _next(context);
    }
}
