using AutoMapper;
using TripFrogModels.Models;
using TripFrogWebApi.Dtos;

namespace TripFrogWebApi;

public sealed class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<User, UserDto>();
    }
}
