using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi.Services;

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
            for (int i = 0; i < UsersData.UsersWithoutPassword.Length; i++)
            {
                PasswordHasher.HashPassword(UsersData.PasswordsForUsers[i], out byte[] salt, out byte[] hash);
                var user = UsersData.UsersWithoutPassword[i];

                user.Id = Guid.NewGuid();
                user.RefreshToken = null;
                user.RefreshTokenId = null;
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                Context.Users.Add(user);
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
