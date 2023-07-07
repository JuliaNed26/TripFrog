using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.ActionsClasses;

public sealed class UserService
{
    private readonly TripFrogContext _context;
    private readonly IMapper _mapper;

    public UserService(TripFrogContext dbContext, IMapper mapper)
    {
        _context = dbContext;
        _mapper = mapper;
    }

    public async Task<IServiceResponse<List<IUserDto>>> GetUsers()
    {
        var users = await _context.Users.Select(user => _mapper.Map<UserDto>(user)).ToListAsync();
        var serviceResponse = new ServiceResponse<List<IUserDto>>
        {
            Data = users.Select(user => user as IUserDto).ToList()
        };
        return serviceResponse;
    }

    public async Task<IServiceResponse<IUserDto>> GetUserById(Guid id)
    {
        var serviceResponse = new ServiceResponse<IUserDto>();

        try
        {
            var foundUser = await _context.Users.SingleAsync(user => user.Id == id);
            serviceResponse.Data = _mapper.Map<UserDto>(foundUser);
        }
        catch (InvalidOperationException)
        {
            serviceResponse.Successful = false;
            serviceResponse.Message = "User with such id was not found";
        }
        return serviceResponse;
    }

    public async Task<IServiceResponse<IUserDto>> RegisterUser(RegisterUserDto registerUser)
    {
        var serviceResponse = new ServiceResponse<IUserDto>();
        
        var usersWithSameLoginExists = await _context.Users.AnyAsync(user => user.Email == registerUser.Email);

        if (usersWithSameLoginExists)
        {
            serviceResponse.Successful = false;
            serviceResponse.Message = "User with such email already exists";

        }
        else
        {
            var newUser = CreateNewUserWithHashedPassword(registerUser);
            ChangeIdWhileGuidExists(newUser);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<UserDto>(_context.Users.Single(user => user.Id == newUser.Id));
        }

        return serviceResponse;

    }

    public async Task<IServiceResponse<IUserDto>> ChangeUserInfo(ChangedUserInfoDto changedUser)
    {
        var serviceResponse = new ServiceResponse<IUserDto>();
        try
        {
            var foundUser = await _context.Users.SingleAsync(user => user.Id == changedUser.Id);
            ChangeExistingUserInfo(changedUser, ref foundUser);

            _context.Users.Update(foundUser);
            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<UserDto>(foundUser);
        }
        catch (InvalidOperationException)
        {
            serviceResponse.Successful = false;
            serviceResponse.Message = "User with such id was not found";
        }
        return serviceResponse;
    }

    public async Task<IServiceResponse<IUserDto>> DeleteUser(Guid id)
    {
        var serviceResponse = new ServiceResponse<IUserDto>();

        try
        {
            var userToDelete = await _context.Users.SingleAsync(user => user.Id == id);
            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<UserDto>(userToDelete);
        }
        catch (InvalidOperationException)
        {
            serviceResponse.Successful = false;
            serviceResponse.Message = "User with such id was not found";
        }
        return serviceResponse;
    }

    private void ChangeExistingUserInfo(ChangedUserInfoDto changedUserDto, ref User changedUser)
    {
        PasswordHasher.HashPassword(changedUserDto.Password, out byte[] passwordSalt, out byte[] passwordHash);

        changedUser.Id = changedUserDto.Id;
        changedUser.FirstName = changedUserDto.FirstName;
        changedUser.LastName = changedUserDto.LastName;
        changedUser.Email = changedUserDto.Email;
        changedUser.Phone = changedUserDto.Phone;
        changedUser.PictureUrl = changedUserDto.PictureUrl;
        changedUser.PasswordSalt = passwordSalt;
        changedUser.PasswordHash = passwordHash;
        changedUser.Role = changedUserDto.Role;
    }

    private User CreateNewUserWithHashedPassword(RegisterUserDto registerUser)
    {
        PasswordHasher.HashPassword(registerUser.Password, out byte[] passwordSalt, out byte[] passwordHash);

        return new User()
        {
            Id = Guid.NewGuid(),
            FirstName = registerUser.FirstName,
            LastName = registerUser.LastName,
            Email = registerUser.Email,
            Phone = registerUser.Phone,
            PasswordSalt = passwordSalt,
            PasswordHash = passwordHash,
            Role = registerUser.Role
        };
    }

    private async void ChangeIdWhileGuidExists(User user)
    {
        while ((await GetUserById(user.Id)).Successful)
        {
            user.Id = Guid.NewGuid();
        }
    }
}
