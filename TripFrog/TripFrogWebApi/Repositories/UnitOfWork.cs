using AutoMapper;
using TripFrogModels;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Services;

namespace TripFrogWebApi.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(TripFrogContext context, IMapper mapper, IJwtTokenService tokenService)
    {
        UserRepository = new UserRepository(context, mapper);
        RefreshTokenRepository = new RefreshTokenRepository(context, tokenService);
    }

    public IUserRepository UserRepository { get; }
    public IRefreshTokenRepository RefreshTokenRepository { get; }

    public async Task<IResponse<ILoginInfoDto>> LoginUser(ILoginUserCredentialsDto loginUserCredentials)
    {
        var foundByCredentialsResponse = await UserRepository.GetUserByCredentialsAsync(loginUserCredentials);

        if (!foundByCredentialsResponse.Successful)
        {
            return new Response<ILoginInfoDto>
            {
                Successful = false,
                Message = foundByCredentialsResponse.Message
            };
        }

        var userWithCredentials = foundByCredentialsResponse.Data;
        await RefreshTokenRepository.DeleteRefreshTokenForUser(userWithCredentials.Id);
        var tokensCreationResponse = await RefreshTokenRepository.CreateRefreshAndJwtTokensForUserAsync((UserDto)userWithCredentials);

        return new Response<ILoginInfoDto>
        {
            Successful = true,
            Data = new LoginInfoDto
            {
                LoggedUser = (UserDto)userWithCredentials,
                Tokens = (TokensDto)tokensCreationResponse.Data

            }
        };
    }

    public async Task LogoutUser(Guid userId)
    {
        await RefreshTokenRepository.DeleteRefreshTokenForUser(userId);
    }
}
