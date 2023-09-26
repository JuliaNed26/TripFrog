namespace TripFrogWebApi.DTO;

public sealed class JwtTokenDto : IJwtTokenDto
{
    public string Token { get; set; }
    public DateTime ExpirationDate { get; set; }
}
