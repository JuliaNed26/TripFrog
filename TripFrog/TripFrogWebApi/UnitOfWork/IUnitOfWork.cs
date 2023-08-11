using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;
namespace TripFrogWebApi;
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    Task<IServiceResponse<Tokens>> LoginUser(LoginUserDto loginUser);
    Task<IServiceResponse<Tokens>> RefreshJwtToken(Tokens oldTokens);
    Task LogoutUser(Guid userId);
}