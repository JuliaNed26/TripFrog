using TripFrogWebApi.DTO;

namespace TripFrogMVC.Services.WebApiClients;

public sealed class RefreshTokenApiClient : ApiClient
{
    public RefreshTokenApiClient(WebApiInfoService webApiInfo, MemoryCacheManagerService cacheManager) : base(webApiInfo, cacheManager) {}

    public async Task<IJwtTokenDto> RefreshJwtTokenForUser(JwtTokenDto jwtToken, UserDto loggedUser)
    {
        var refreshTokenForUser = await GetRefreshTokenForUser(loggedUser.Id);
        var tokens = new TokensDto
        {
            JwtToken = jwtToken,
            RefreshToken = refreshTokenForUser
        };

        using var client = HttpClientFactory.GetHttpClient();
        var refreshJwtTokenResponseJson = await client.PostAsJsonAsync("api/RefreshToken/refreshUserToken", tokens);
        var refreshJwtTokenResponse = await refreshJwtTokenResponseJson.DeserializeResponseAsync<Response<TokensDto>>();
        if (!refreshJwtTokenResponse.Successful)
        {
            throw new InvalidOperationException(refreshJwtTokenResponse.Message);
        }

        return refreshJwtTokenResponse.Data.JwtToken;
    }

    private async Task<string> GetRefreshTokenForUser(Guid userId)
    {
        using var client = HttpClientFactory.GetHttpClient();
        var response = await client.GetFromJsonAsync<Response<string>>($"api/RefreshToken/{userId}");
        if (response == null || !response.Successful)
        {
            throw new InvalidOperationException("Can not get refresh token. User should log in again");
        }

        return response.Data;
    }
}