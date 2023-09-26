namespace TripFrogWebApi.DTO;

public interface IJwtTokenDto
{
    public string Token { get; }
    public DateTime ExpirationDate { get; }
}
