namespace TripFrogWebApi.DTO;

public interface ITokensDto
{
    public JwtTokenDto JwtToken { get; }
    public string RefreshToken { get; }
}
