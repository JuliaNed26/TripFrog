using AutoMapper;
using NSubstitute;
using TripFrogModels.Models;
using TripFrogWebApi.DTO;

namespace TripFrogFixtures.RepositoryTests;

internal static class MapperSubstitudeConfigurer
{
    public static IMapper GetConfiguredMapper()
    {
        IMapper mapper = Substitute.For<IMapper>();
        mapper.Map<UserDto>(Arg.Any<User>())
            .ReturnsForAnyArgs(methodParams =>
            {
                User user = methodParams.Arg<User>();
                return new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    PictureUrl = user.PictureUrl,
                    Role = user.Role
                };
            });
        return mapper;
    }
}
