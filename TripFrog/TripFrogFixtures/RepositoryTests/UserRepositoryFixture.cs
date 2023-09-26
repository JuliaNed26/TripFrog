using Microsoft.EntityFrameworkCore;
using TripFrogModels;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;
using TripFrogWebApi.Services;

namespace TripFrogFixtures.RepositoryTests
{
    [TestFixture]
    internal sealed class UserRepositoryFixture
    {
        private static readonly RegisterUserDto[] NewUsersData =
        {
            new()
            {
                FirstName = "Ekler",
                LastName = "Nedavni",
                Email = "newEmail@gmail.com",
                Password = "password1",
                Role = Role.Landlord
            },
        };

        private static readonly ChangedUserInfoDto[] ChangesToUserData =
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "NewName",
                LastName = "NewLastName",
                Email = "newEmail@gmail.com",
                Phone = "NewPhone",
                PictureUrl = "newPicture",
                Password = "newPassword1",
                Role = Role.Landlord
            }
        };

        private readonly DatabaseService _dbService = new();
        private IUserRepository _userRepository;

        [OneTimeSetUp]
        public void SetupFields()
        {
            var mapper = MapperSubstitudeConfigurer.GetConfiguredMapper();
            _userRepository = new UserRepository(_dbService.Context, mapper);
        }

        [TestCaseSource(nameof(NewUsersData))]
        public async Task RegisterUser_UserWithSuchEmailDoesNotExistYet_AddUserReturnUserInfo(RegisterUserDto newUser)
        {
            //Act
            var response = await _userRepository.RegisterUserAsync(newUser);

            //Assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data.FirstName, Is.EqualTo(newUser.FirstName));
            Assert.That(response.Data.LastName, Is.EqualTo(newUser.LastName));
            Assert.That(response.Data.Email, Is.EqualTo(newUser.Email));
            Assert.That(response.Data.Phone, Is.EqualTo(newUser.Phone));
            Assert.That(response.Data.Role, Is.EqualTo(newUser.Role));
            Assert.That(_dbService.Context.Users.Count(user => user.Email == response.Data.Email), Is.EqualTo(1));
        }

        [Test]
        public async Task RegisterUser_UserWithSuchEmailAlreadyExists_ReturnsUnsuccessfulResponseUserNotAdded()
        {
            //Arrange
            _dbService.FillUsersTable();

            var registeredEmail = await _dbService.Context.Users.Select(user => user.Email).FirstAsync();
            RegisterUserDto userWithRegisteredEmail = new()
            {
                FirstName = "Julia",
                LastName = "Nedavnia",
                Email = registeredEmail,
                Password = "param25",
                Role = Role.Landlord
            };

            //Act
            var response = await _userRepository.RegisterUserAsync(userWithRegisteredEmail);
            var usersWithEmailCount = _dbService.Context.Users.Count(user => user.Email == userWithRegisteredEmail.Email);

            //Assert
            Assert.IsFalse(response.Successful);
            Assert.That(usersWithEmailCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUsers_ReturnsListOfUsers()
        {
            //Arrange
            _dbService.FillUsersTable();
            var countOfUsersInDatabase = _dbService.Context.Users.Count();

            //Act
            var response = await _userRepository.GetUsersAsync();

            //Assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data.Count, Is.EqualTo(countOfUsersInDatabase));
        }

        [Test]
        public async Task GetUsers_NoRegisteredUsersYet_ReturnsEmptyList()
        {
            //Act
            var response = await _userRepository.GetUsersAsync();

            //Assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data.Count, Is.EqualTo(0));
        }
        
        [TestCaseSource(nameof(ChangesToUserData))]
        public async Task ChangeUserInfo_UserWithIdExists_UpdatesUserInformation(ChangedUserInfoDto changedUser)
        { 
            //Arrange
            _dbService.FillUsersTable();

            var firstUser = await _dbService.Context.Users.FirstAsync();
            changedUser.Id = firstUser.Id;

            //act
            var response = await _userRepository.ChangeUserInfoAsync(changedUser);
            var updatedUser = await _dbService.Context.Users.FirstAsync();

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(updatedUser.FirstName, Is.EqualTo(changedUser.FirstName));
            Assert.That(updatedUser.LastName, Is.EqualTo(changedUser.LastName));
            Assert.That(updatedUser.Email, Is.EqualTo(changedUser.Email));
            Assert.That(updatedUser.Phone, Is.EqualTo(changedUser.Phone));
            Assert.That(updatedUser.PictureUrl, Is.EqualTo(changedUser.PictureUrl));
            Assert.That(updatedUser.Role, Is.EqualTo(changedUser.Role));
        }

        [TestCaseSource(nameof(ChangesToUserData))]
        public async Task ChangeUserInfo_UserNotExists_ReturnsUnsuccessfulResponse(ChangedUserInfoDto changedUser)
        {
            //Arrange
            changedUser.Id = Guid.NewGuid();

            //Act
            var response = await _userRepository.ChangeUserInfoAsync(changedUser);

            //Assert
            Assert.IsFalse(response.Successful);
        }

        [TestCase("newPassword1")]
        public async Task ChangeUserInfo_SetNewPassword_ChangesUserPassword(string newPassword)
        {
            //Arrange
            _dbService.FillUsersTable();

            var firstUser = await _dbService.Context.Users.FirstAsync();
            var changedUser = new ChangedUserInfoDto
            {
                Id = firstUser.Id, 
                Password = newPassword
            };

            //Act
            var response = await _userRepository.ChangeUserInfoAsync(changedUser);
            var updatedUser = await _dbService.Context.Users.FirstAsync();

            //Assert
            Assert.IsTrue(response.Successful); 
            Assert.IsTrue(PasswordHasher.IsPasswordValidBySaltAndHash(newPassword, updatedUser.PasswordSalt, updatedUser.PasswordHash));
        }

        [Test]
        public async Task DeleteUser_UserWithIdExists_DeletesUserReturnsUserInfo()
        {
            //Arrange
            _dbService.FillUsersTable();
            var firstUser = _dbService.Context.Users.First();

            //Act
            var response = await _userRepository.DeleteUserAsync(firstUser.Id);
            var userExists = _dbService.Context.Users.Any(user => user.Id == firstUser.Id);

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data.Id, Is.EqualTo(firstUser.Id));
            Assert.That(response.Data.FirstName, Is.EqualTo(firstUser.FirstName));
            Assert.That(response.Data.LastName, Is.EqualTo(firstUser.LastName));
            Assert.That(response.Data.Email, Is.EqualTo(firstUser.Email));
            Assert.That(response.Data.Phone, Is.EqualTo(firstUser.Phone));
            Assert.That(response.Data.PictureUrl, Is.EqualTo(firstUser.PictureUrl));
            Assert.That(response.Data.Role, Is.EqualTo(firstUser.Role));
            Assert.IsFalse(userExists);
        }

        [Test]
        public async Task DeleteUser_UserWithIdNotExists_ReturnsUnsuccessfulResponse()
        {
            //Act
            var response = await _userRepository.DeleteUserAsync(Guid.NewGuid());

            //Assert
            Assert.IsFalse(response.Successful);
        }

        [TearDown]
        public void ClearUsersTable()
        {
            _dbService.ClearDatabase();
        }
    }
}
