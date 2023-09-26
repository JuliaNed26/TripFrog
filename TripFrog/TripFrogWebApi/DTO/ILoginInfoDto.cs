namespace TripFrogWebApi.DTO;

public interface ILoginInfoDto
{
    public TokensDto Tokens { get; }
    public UserDto LoggedUser { get; }
}
