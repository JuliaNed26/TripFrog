using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi;

namespace TripFrogFixtures
{
    internal sealed class DatabaseService
    {
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
            foreach (var user in usersTableData)
            {
                Context.Users.Add(user);
            }

            Context.SaveChanges();
        }

        public void RemoveAllUsers()
        {
            Context.Users.RemoveRange(Context.Users.ToList());
        }

        private static readonly User[] usersTableData =
        {
            new()
            {
                FirstName = "Slava",
                LastName = "Nedavni",
                Email = "email1@gmail.com",
                PasswordHash = HashPassword("crakozyabra0", out byte[] salt),
                PasswordSalt = salt,
                Role = Role.Traveler
            },
            new()
            {
                FirstName = "Olya",
                LastName = "Demchenko",
                Email = "email2@gmail.com",
                PasswordHash = HashPassword("piupes12", out salt),
                PasswordSalt = salt,
                Role = Role.Traveler
            },
            new()
            {
                FirstName = "Liuda",
                LastName = "Nedavnia",
                Email = "email3@gmail.com",
                PasswordHash = HashPassword("kitten18", out salt),
                PasswordSalt = salt,
                Role = Role.Traveler
            },
        };

        private static byte[] HashPassword(string password, out byte[] salt)
        {
            PasswordHasher.HashPassword(password, out byte[] pasHash, out byte[] pasSalt);
            salt = pasSalt;
            return pasHash;
        }

    }
}
