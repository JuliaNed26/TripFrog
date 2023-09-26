using TripFrogModels;
using TripFrogModels.Models;

namespace TripFrogFixtures;

internal static class UsersData
{
    public static string[] PasswordsForUsers =>
        new[]
        {
            "crakozyabra0", "piupes12", "kitten18"
        };

    public static User[] UsersWithoutPassword => new []
    {
        new User()
        {
            FirstName = "Slava",
            LastName = "Nedavni",
            Email = "email1@gmail.com",
            Role = Role.Traveler
        },
        new User()
        {
            FirstName = "Olya",
            LastName = "Demchenko",
            Email = "email2@gmail.com",
            Role = Role.Traveler
        },
        new User()
        {
            FirstName = "Liuda",
            LastName = "Nedavnia",
            Email = "email3@gmail.com",
            Role = Role.Traveler
        },
    };
}
