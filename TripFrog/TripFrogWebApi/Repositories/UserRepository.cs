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

    public async Task<IResponse<List<IUserDto>>> GetUsersAsync()
    {
        var users = await _context.Users.Select(user => (IUserDto)_mapper.Map<UserDto>(user))
                                                     .ToListAsync();

        return new Response<List<IUserDto>>
        {
            Successful = true,
            Data = users
        };
    }

    public async Task<IResponse<IUserDto>> GetUserByCredentialsAsync(ILoginUserCredentialsDto loginCredentials)
    {
        var userWithEmail = await _context.Users.SingleOrDefaultAsync(user => user.Email == loginCredentials.Email);
        if (userWithEmail == null)
        {
            return new Response<IUserDto>
            {
                Successful = false,
                Message = "User with such email does not exist"
            };
        }

        if (!PasswordHasher.IsPasswordValidBySaltAndHash(loginCredentials.Password, 
                                                         userWithEmail.PasswordSalt,
                                                         userWithEmail.PasswordHash))
        {
            return new Response<IUserDto>
            {
                Successful = false,
                Message = "Password is not correct"
            };
        }

        return new Response<IUserDto>
        {
            Successful = true,
            Data = _mapper.Map<UserDto>(userWithEmail)
        };
    }

    public async Task<IResponse<IUserDto>> RegisterUserAsync(IRegisterUserDto registerUser)
    {
        var usersWithSameEmailExists = await _context.Users.AnyAsync(user => user.Email == registerUser.Email);

        if (usersWithSameEmailExists)
        {
            return new Response<IUserDto>
            {
                Successful = false,
                Message = "User with such email already exists"
            };
        }

        var newUser = CreateNewUserWithHashedPassword(registerUser);
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var createdUser = _context.Users.Single(user => user.Id == newUser.Id);
        return new Response<IUserDto>
        {
            Successful = true,
            Data = _mapper.Map<UserDto>(createdUser)
        };
    }

    public async Task<IResponse<IUserDto>> ChangeUserInfoAsync(IChangedUserInfoDto changedUser)
    {
        var userToChange = await _context.Users.SingleOrDefaultAsync(user => user.Id == changedUser.Id);
        if (userToChange == null)
        {
            return new Response<IUserDto>
            {
                Successful = false,
                Message = "User with such id was not found"
            };
        }

        ChangeUserEntity(changedUser, ref userToChange);
        _context.Users.Update(userToChange);
        await _context.SaveChangesAsync();

        return new Response<IUserDto>
        {
            Successful = true,
            Data = _mapper.Map<UserDto>(userToChange)
        };
    }

    public async Task<IResponse<IUserDto>> DeleteUserAsync(Guid id)
    {
        var userToDelete = await _context.Users.SingleOrDefaultAsync(user => user.Id == id);
        if (userToDelete == null)
        {
            return new Response<IUserDto>
            {
                Successful = false,
                Message = "User with such id was not found"
            };
        }
        _context.Users.Remove(userToDelete);
        await _context.SaveChangesAsync();

        return new Response<IUserDto>
        {
            Successful = true,
            Data = _mapper.Map<UserDto>(userToDelete)
        };
    }

    private static void ChangeUserEntity(IChangedUserInfoDto changesToUser, ref User userEntity)
    {
        userEntity.FirstName = changesToUser.FirstName;
        userEntity.LastName = changesToUser.LastName;
        userEntity.Email = changesToUser.Email;
        userEntity.Phone = changesToUser.Phone;
        userEntity.PictureUrl = changesToUser.PictureUrl;
        userEntity.Role = changesToUser.Role;

        if (changesToUser.Password is not null)
        {
            PasswordHasher.HashPassword(changesToUser.Password, out byte[] passwordSalt, out byte[] passwordHash);
            userEntity.PasswordSalt = passwordSalt;
            userEntity.PasswordHash = passwordHash;
        }
    }

    private User CreateNewUserWithHashedPassword(IRegisterUserDto registerUser)
    {
        PasswordHasher.HashPassword(registerUser.Password, out byte[] passwordSalt, out byte[] passwordHash);

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
