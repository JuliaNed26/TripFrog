using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Repositories;

public interface IUserRepository
{
     Task<IServiceResponse<List<IUserDto>>> GetUsers();

     Task<IServiceResponse<IUserDto>> GetUserById(Guid id);

     Task<IServiceResponse<IUserDto>> RegisterUser(RegisterUserDto registerUser);

     Task<IServiceResponse<IUserDto>> ChangeUserInfo(ChangedUserInfoDto changedUser);

     Task<IServiceResponse<IUserDto>> DeleteUserAsync(Guid id);
}
