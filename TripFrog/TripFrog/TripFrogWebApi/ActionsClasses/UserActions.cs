using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi.Dtos;

namespace TripFrogWebApi.ActionsClasses;

public sealed class UserActions
{
    private TripFrogContext _context;
    private IMapper _mapper;

    public UserActions(TripFrogContext dbContext, IMapper mapper)
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
        
        var usersWithSameLoginExists = await _context.Users.AnyAsync(user => user.Login == registerUser.Login);

        if (usersWithSameLoginExists)
        {
            serviceResponse.Successful = false;
            serviceResponse.Message = "User with such login exists";

        }
        else
        {

            byte[] passwordSalt, passwordHash;
            PasswordHasher.HashPassword(registerUser.Password, out passwordSalt, out passwordHash);

            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = registerUser.FirstName,
                LastName = registerUser.LastName,
                Email = registerUser.Email,
                Phone = registerUser.Phone,
                Login = registerUser.Login,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                Role = registerUser.Role
            };

            while ((await GetUserById(newUser.Id)).Successful)
            {
                newUser.Id = Guid.NewGuid();
            }
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<UserDto>(_context.Users.Single(user => user.Id == newUser.Id));
        }

        return serviceResponse;
    }

    /*public async Task<IServiceResponse<IUserDto>> ChangeUserInfo(RegisterUserDto registerUser)
    {
        var serviceResponse = new ServiceResponse<IUserDto>();

        try
        {
            var foundUser = await _context.Users.SingleAsync(user => user.Id == registerUser.Id);
            foundUser.Id = registerUser.Id;
            foundUser.FirstName = registerUser.FirstName;
            foundUser.LastName = registerUser.LastName;
            foundUser.Email = registerUser.Email;
            foundUser.Phone = registerUser.Phone;
            foundUser.PictureUrl = registerUser.PictureUrl;
            foundUser.Login = registerUser.Login;
            foundUser.Password = registerUser.Password;
            foundUser.Role = registerUser.Role;
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
    }*/

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
}
