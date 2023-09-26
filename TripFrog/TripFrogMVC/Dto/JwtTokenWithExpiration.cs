namespace TripFrogMVC.Dto;

internal sealed class JwtTokenWithExpiration
{
    public string JwtToken { get; set; }
    public DateTime ExpirationDate { get; set; }
}
