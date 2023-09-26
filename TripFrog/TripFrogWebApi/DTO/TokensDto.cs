namespace TripFrogWebApi.DTO;

public sealed class TokensDto : ITokensDto
{
    public JwtTokenDto JwtToken { get; set; }
    public string RefreshToken { get; set; }
}
