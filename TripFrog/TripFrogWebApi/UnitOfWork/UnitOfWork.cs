using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using TripFrogModels;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;
using TripFrogWebApi.Services;
using TripFrogWebApi.TokensCreator;

namespace TripFrogWebApi;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TripFrogContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtTokenService _tokenService;

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private IUserRepository? _userRepository;

    public UnitOfWork(TripFrogContext context, IMapper mapper, IJwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
        _mapper = mapper;
        _refreshTokenRepository = new RefreshTokenRepository(context, tokenService);
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context, _mapper);

    public async Task<IServiceResponse<Tokens>> LoginUser(LoginUserDto loginUser)
    {
        var serviceResponse = new ServiceResponse<Tokens>();
        var userWithEmail = await _context.Users.SingleOrDefaultAsync(user => user.Email == loginUser.Email);

        if (userWithEmail == null)
        {
            serviceResponse.Successful = false;
            serviceResponse.Message = "User with such email does not exist";
        }
        else if (!PasswordHasherService.VerifyPassword(loginUser.Password, userWithEmail.PasswordSalt,
                                                userWithEmail.PasswordHash))
        {
            serviceResponse.Successful = false;
            serviceResponse.Message = "Password is not correct";
        }
        else
        {
            serviceResponse.Data = await _refreshTokenRepository.CreateNewTokensForUserAsync(_mapper.Map<UserDto>(userWithEmail));
        }
        return serviceResponse;
    }

    public async Task LogoutUser(Guid userId)
    {
        await _refreshTokenRepository.DeleteRefreshTokenForUser(userId);
    }

    public async Task<IServiceResponse<Tokens>> RefreshJwtToken(Tokens oldTokens)
    {
        var response = new ServiceResponse<Tokens>();

        if (_tokenService.TryGetClaimsFromToken(oldTokens.JwtToken, out var tokenClaimsPrincipal))
        {
            var expiryDateUnix = long.Parse(tokenClaimsPrincipal!.Claims
                                                                      .Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDateUnix);

            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                response.Successful = false;
                response.Message = "Jwt token has not expired yet";
            }
            else
            {
                var userId = tokenClaimsPrincipal.Claims.Single(x => x.Type == "Id").Value;
                var user = _context.Users.First(user => user.Id.ToString() == userId);

                var refreshTokenValidationResult = await _refreshTokenRepository.TryValidateRefreshTokenAsync(oldTokens.RefreshToken);
                response.Successful = refreshTokenValidationResult.successful;

                if (refreshTokenValidationResult.successful)
                {
                    response.Data = new Tokens
                    {
                        JwtToken = _tokenService.GenerateJwtToken(_mapper.Map<UserDto>(user)),
                        RefreshToken = oldTokens.RefreshToken,
                    };
                }
                else
                {
                    response.Message = refreshTokenValidationResult.message;
                }
            }
        }
        return response;
    }
}
