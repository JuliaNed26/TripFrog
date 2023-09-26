namespace TripFrogWebApi.DTO;
public sealed class LoginInfoDto : ILoginInfoDto
{
    public TokensDto Tokens { get; set; }
    public UserDto LoggedUser { get; set; }
}
