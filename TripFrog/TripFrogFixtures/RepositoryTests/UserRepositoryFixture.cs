using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TripFrogModels;
using TripFrogModels.Models;
using TripFrogWebApi;
using TripFrogWebApi.DTO;
using TripFrogWebApi.Repositories;

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
                Password = "passsword1",
                Role = Role.Admin
            },
        };

        private static readonly ChangedUserInfoDto[] ChangesToUserData =
        {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "NewName",
            },
            new()
            {
                Id = Guid.NewGuid(),
                LastName = "NewLastName",
                Email = "newEmail@gmail.com",
                Phone = "NewPhone",
                PictureUrl = "newPicture",
                Role = Role.Admin
            },
        };

        private readonly DatabaseService _dbService = new ();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private IUserRepository _userRepository;

        [OneTimeSetUp]
        public void SetupFields()
        {
            _mapper.Map<UserDto>(Arg.Any<User>())
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

            _userRepository = new UserRepository(_dbService.Context, _mapper);
        }

        [TestCaseSource(nameof(NewUsersData))]
        public async Task RegisterUser_UserWithSuchEmailDoesNotExistYet_ReturnsUserInfoAddsUser(RegisterUserDto newUser)
        {
            //act
            var response = await _userRepository.RegisterUser(newUser);

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.FirstName, Is.EqualTo(newUser.FirstName));
            Assert.That(response.Data!.LastName, Is.EqualTo(newUser.LastName));
            Assert.That(response.Data!.Email, Is.EqualTo(newUser.Email));
            Assert.That(response.Data!.Phone, Is.EqualTo(newUser.Phone));
            Assert.That(response.Data!.Role, Is.EqualTo(newUser.Role));
            Assert.That(_dbService.Context.Users.Count(user => user.Email == response.Data!.Email), Is.EqualTo(1));
        }

        [Test]
        public async Task RegisterUser_UserWithSuchEmailAlreadyExists_ReturnsUnsuccessfulResponseUserNotAdded()
        {
            //arrange
            _dbService.FillUsersTable();
            var registeredEmail = await _dbService.Context.Users.Select(user => user.Email).FirstAsync();
            RegisterUserDto userWithRegisteredEmail = new()
            {
                FirstName = "Julia",
                LastName = "Nedavnia",
                Email = registeredEmail,
                Password = "param25",
                Role = Role.Admin
            };

            //act
            var response = await _userRepository.RegisterUser(userWithRegisteredEmail);

            //assert
            Assert.IsFalse(response.Successful);
            Assert.That(_dbService.Context.Users.Count(user => user.Email == userWithRegisteredEmail.Email), Is.EqualTo(1));
        }

        [Test]
        public async Task GetUsers_ReturnsListOfUsers()
        {
            //arrange
            _dbService.FillUsersTable();

            //act
            var response = await _userRepository.GetUsers();

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Count, Is.EqualTo(_dbService.Context.Users.Count()));
        }

        [Test]
        public async Task GetUsers_NoRegisteredUsersYet_ReturnsEmptyList()
        {
            //act
            var response = await _userRepository.GetUsers();

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUserById_UserWithIdExists_ReturnsFoundUser()
        {
            //arrange
            _dbService.FillUsersTable();
            var existUser = await _dbService.Context.Users
                                                    .FirstAsync();

            //act
            var response = await _userRepository.GetUserById(existUser.Id);

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Id, Is.EqualTo(existUser.Id));
            Assert.That(response.Data!.FirstName, Is.EqualTo(existUser.FirstName));
            Assert.That(response.Data!.LastName, Is.EqualTo(existUser.LastName));
            Assert.That(response.Data!.Email, Is.EqualTo(existUser.Email));
            Assert.That(response.Data!.Phone, Is.EqualTo(existUser.Phone));
            Assert.That(response.Data!.PictureUrl, Is.EqualTo(existUser.PictureUrl));
            Assert.That(response.Data!.Role, Is.EqualTo(existUser.Role));
        }

        [Test]
        public async Task GetUserById_UserWithIdNotExists_ReturnsUnsuccessfulResponse()
        {
            //act
            var response = await _userRepository.GetUserById(Guid.NewGuid());

            //assert
            Assert.IsFalse(response.Successful);
        }

        [TestCaseSource(nameof(ChangesToUserData))]
        public async Task ChangeUserInfo_UserWithIdExists_UpdatesUserInformation(ChangedUserInfoDto changedUser)
        { 
            //arrange
            _dbService.FillUsersTable();
            var firstUser = await _dbService.Context.Users.FirstAsync();
            changedUser.Id = firstUser.Id;

            //act
            var response = await _userRepository.ChangeUserInfo(changedUser);

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.FirstName, Is.EqualTo(changedUser.FirstName ?? firstUser.FirstName));
            Assert.That(response.Data!.LastName, Is.EqualTo(changedUser.LastName ?? firstUser.LastName));
            Assert.That(response.Data!.Email, Is.EqualTo(changedUser.Email ?? firstUser.Email));
            Assert.That(response.Data!.Phone, Is.EqualTo(changedUser.Phone ?? firstUser.Phone));
            Assert.That(response.Data!.PictureUrl, Is.EqualTo(changedUser.PictureUrl ?? firstUser.PictureUrl));
            Assert.That(response.Data!.Role, Is.EqualTo(changedUser.Role ?? firstUser.Role));
        }

        [TestCaseSource(nameof(ChangesToUserData))]
        public async Task ChangeUserInfo_UserNotExists_ReturnsUnsuccessfulResponse(ChangedUserInfoDto changedUser)
        {
            //arrange
            changedUser.Id = Guid.NewGuid();

            //act
            var response = await _userRepository.ChangeUserInfo(changedUser);

            //assert
            Assert.IsFalse(response.Successful);
        }

        [TestCase("newPassword1")]
        public async Task ChangeUserInfo_SetNewPassword_ChangesUserPassword(string newPassword)
        {
            //arrange
            _dbService.FillUsersTable();
            var firstUser = await _dbService.Context.Users.FirstAsync();
            var changedUser = new ChangedUserInfoDto
            {
                Id = firstUser.Id, 
                Password = newPassword
            };

            //act
            var response = await _userRepository.ChangeUserInfo(changedUser);
            var updatedUser = await _dbService.Context.Users.FirstAsync();

            //assert
            Assert.IsTrue(response.Successful); 
            Assert.IsTrue(PasswordHasherService.VerifyPassword(newPassword, updatedUser.PasswordSalt, updatedUser.PasswordHash));
        }

        [Test]
        public async Task DeleteUser_UserWithIdExists_ReturnsUserInfoDeletesUser()
        {
            //arrange
            _dbService.FillUsersTable();
            var firstUser = _dbService.Context.Users.First();

            //act
            var response = await _userRepository.DeleteUserAsync(firstUser.Id);

            //assert
            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Id, Is.EqualTo(firstUser.Id));
            Assert.That(response.Data!.FirstName, Is.EqualTo(firstUser.FirstName));
            Assert.That(response.Data!.LastName, Is.EqualTo(firstUser.LastName));
            Assert.That(response.Data!.Email, Is.EqualTo(firstUser.Email));
            Assert.That(response.Data!.Phone, Is.EqualTo(firstUser.Phone));
            Assert.That(response.Data!.PictureUrl, Is.EqualTo(firstUser.PictureUrl));
            Assert.That(response.Data!.Role, Is.EqualTo(firstUser.Role));
            Assert.IsFalse(_dbService.Context.Users.Any(user => user.Id == firstUser.Id));
        }

        [Test]
        public async Task DeleteUser_UserWithIdNotExists_ReturnsUnsuccessfulResponse()
        {
            //act
            var response = await _userRepository.DeleteUserAsync(Guid.NewGuid());

            //assert
            Assert.IsFalse(response.Successful);
        }

        [TearDown]
        public void ClearUsersTable()
        {
            _dbService.ClearDatabase();
        }
    }
}
