using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Repositories;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }

    Task<IResponse<ILoginInfoDto>> LoginUser(ILoginUserCredentialsDto loginUserCredentials);
    Task LogoutUser(Guid userId);
}
