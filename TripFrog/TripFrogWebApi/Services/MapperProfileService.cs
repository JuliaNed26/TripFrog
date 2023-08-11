using AutoMapper;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Services;

public sealed class MapperProfileService : Profile
{
    public MapperProfileService()
    {
        CreateMap<User, UserDto>();
    }
}
