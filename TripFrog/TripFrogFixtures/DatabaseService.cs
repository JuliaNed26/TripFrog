using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi;

namespace TripFrogFixtures
{
    internal sealed class DatabaseService
    {
        private static readonly string[] PasswordsForUsersTable = { "crakozyabra0", "piupes12", "kitten18" };
        private static readonly User[] UsersTableData =
        {
            new()
            {
                FirstName = "Slava",
                LastName = "Nedavni",
                Email = "email1@gmail.com",
                Role = Role.Traveler
            },
            new()
            {
                FirstName = "Olya",
                LastName = "Demchenko",
                Email = "email2@gmail.com",
                Role = Role.Traveler
            },
            new()
            {
                FirstName = "Liuda",
                LastName = "Nedavnia",
                Email = "email3@gmail.com",
                Role = Role.Traveler
            },
        };

        public DatabaseService()
        {
            var options = new DbContextOptionsBuilder<TripFrogContext>()
                             .UseInMemoryDatabase(databaseName: "InMemoryDb")
                             .Options;

            Context = new TripFrogContext(options);
        }

        public TripFrogContext Context { get; init; }

        public void FillUsersTable()
        {
            int i = 0;

            foreach (var user in UsersTableData)
            {
                PasswordHasherService.HashPassword(PasswordsForUsersTable[i], out byte[] salt, out byte[] hash);

                user.Id = Guid.NewGuid();
                user.RefreshToken = null;
                user.RefreshTokenId = null;
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                Context.Users.Add(user);

                i++;
            }

            Context.SaveChanges();
        }

        public void ClearDatabase()
        {
            Context.RefreshTokens.RemoveRange(Context.Set<RefreshToken>());
            Context.Users.RemoveRange(Context.Set<User>());
            Context.SaveChanges();
        }
    }
}
