using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Repositories;

public interface IUserRepository
{
     Task<IResponse<List<IUserDto>>> GetUsersAsync();

     Task<IResponse<IUserDto>> GetUserByCredentialsAsync(ILoginUserCredentialsDto loginCredentials);

     Task<IResponse<IUserDto>> RegisterUserAsync(IRegisterUserDto registerUser);

     Task<IResponse<IUserDto>> ChangeUserInfoAsync(IChangedUserInfoDto changedUser);

     Task<IResponse<IUserDto>> DeleteUserAsync(Guid id);
}
