using AutoMapper;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;

namespace TripFrogWebApi;

public sealed class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<User, UserDto>();
    }
}
