namespace TripFrogWebApi.DTO;

public sealed class LoginUserCredentialsDto : ILoginUserCredentialsDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
