using Microsoft.Extensions.Caching.Memory;
using TripFrogWebApi.DTO;

namespace TripFrogMVC.Services;

internal static class CacheKeys
{
    public const string LoggedUser = "LoggedUser";
    public const string JwtToken = "AuthorizationId";
}

public sealed class MemoryCacheManagerService
{
    private readonly IMemoryCache _memoryCache;
    public MemoryCacheManagerService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public IJwtTokenDto? GetJwtToken()
    {
        _memoryCache.TryGetValue(CacheKeys.JwtToken, out IJwtTokenDto? token);
        return token;
    }

    public void SetJwtToken(IJwtTokenDto jwtToken) => _memoryCache.Set(CacheKeys.JwtToken, jwtToken);


    public IUserDto GetLoggedUser()
    {
        _memoryCache.TryGetValue(CacheKeys.LoggedUser, out IUserDto? loggedUser);
        if (loggedUser == null)
        {
            throw new ArgumentNullException(nameof(loggedUser));
        }
        return loggedUser;
    }

    public void SetLoggedUser(IUserDto loggedUser) => _memoryCache.Set(CacheKeys.LoggedUser, loggedUser);
}
