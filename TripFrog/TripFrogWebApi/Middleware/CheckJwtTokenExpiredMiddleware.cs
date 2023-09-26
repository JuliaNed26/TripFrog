using TripFrogWebApi.Services;

namespace TripFrogWebApi.Middleware;

public sealed class CheckJwtTokenExpiredMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenService _jwtTokenService;

    public CheckJwtTokenExpiredMiddleware(RequestDelegate next, IJwtTokenService jwtTokenService)
    {
        _next = next;
        _jwtTokenService = jwtTokenService;
    }
    public async Task Invoke(HttpContext context)
    {
        var jwtToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(jwtToken))
        {
            _jwtTokenService.TryGetClaimsFromToken(jwtToken, out var tokenClaims);
            bool isTokenExpired = _jwtTokenService.GetTokenExpirationDate(tokenClaims) < DateTime.UtcNow;

            if (isTokenExpired)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("JWT token has expired.");
                return;
            }
        }

        await _next(context);
    }
}
