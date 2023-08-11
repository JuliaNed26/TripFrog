using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Services;

namespace TripFrogWebApi.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly TripFrogContext _context;
    private readonly IMapper _mapper;

    public UserRepository(TripFrogContext dbContext, IMapper mapper)
    {
        _context = dbContext;
        _mapper = mapper;
    }

    public async Task<IServiceResponse<List<IUserDto>>> GetUsers()
    {
        var users = await _context.Users.Select(user => _mapper.Map<UserDto>(user)).ToListAsync();
        var serviceResponse = new ServiceResponse<List<IUserDto>>
        {
            Successful = true,
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

    public async Task<IServiceResponse<IUserDto>> DeleteUserAsync(Guid id)
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

    private static void ChangeExistingUserInfo(ChangedUserInfoDto changesToUser, ref User userEntityToChange)
    {
        userEntityToChange.Id = changesToUser.Id;
        userEntityToChange.FirstName = changesToUser.FirstName ?? userEntityToChange.FirstName;
        userEntityToChange.LastName = changesToUser.LastName ?? userEntityToChange.LastName;
        userEntityToChange.Email = changesToUser.Email ?? userEntityToChange.Email;
        userEntityToChange.Phone = changesToUser.Phone ?? userEntityToChange.Phone;
        userEntityToChange.PictureUrl = changesToUser.PictureUrl ?? userEntityToChange.PictureUrl;
        userEntityToChange.Role = changesToUser.Role ?? userEntityToChange.Role;

        if (changesToUser.Password != null)
        {
            PasswordHasherService.HashPassword(changesToUser.Password, out byte[] passwordSalt, out byte[] passwordHash);
            userEntityToChange.PasswordSalt = passwordSalt;
            userEntityToChange.PasswordHash = passwordHash;
        }
    }

    private User CreateNewUserWithHashedPassword(RegisterUserDto registerUser)
    {
        PasswordHasherService.HashPassword(registerUser.Password, out byte[] passwordSalt, out byte[] passwordHash);

        return new User
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
}
