using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
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
        private DatabaseService _dbService = new DatabaseService();
        private UserRepository _userRepository = default!;
        private Mock<IMapper> _mapper = new ();
        private Mock<JWTTokenCreator> _jwtTokenCreator;

        [OneTimeSetUp]
        public void SetupMocks()
        {
            _jwtTokenCreator = new Mock<JWTTokenCreator>("key");

            _mapper.Setup(mapper => mapper.Map<UserDto>(It.IsAny<User>()))
                .Returns((User user) => new UserDto()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    PictureUrl = user.PictureUrl,
                    Role = user.Role
                });

            _userRepository = new UserRepository(_dbService.Context, _mapper.Object, _jwtTokenCreator.Object);
        }

        [Test]
        public async Task GetUsers_UsersExists_ReturnsListOfUsers()
        {
            _dbService.FillUsersTable();

            var response = await _userRepository.GetUsers();

            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(_dbService.Context.Users.Count(), Is.EqualTo(response.Data!.Count));
        }

        [Test]
        public async Task GetUsers_UsersNotExist_ReturnsEmptyList()
        {
            var response = await _userRepository.GetUsers();

            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUserById_UserWithIdExists_ReturnsUser()
        {
            _dbService.FillUsersTable();
            var existUserId = _dbService.Context.Users.Select(user => user.Id)
                                                           .FirstOrDefault();

            var response = await _userRepository.GetUserById(existUserId);

            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Id, Is.EqualTo(existUserId));
        }

        [Test]
        public async Task GetUserById_UserWithIdNotExist_ResponseIsNotSuccessful()
        {
            var response = await _userRepository.GetUserById(Guid.NewGuid());

            Assert.IsFalse(response.Successful);
            Assert.That(response.Data, Is.Null);
        }

        [TestCaseSource(nameof(usersToRegister))]
        public async Task RegisterUser_UserWithEmailDoesNotExist_ShouldRegister(RegisterUserDto newUser)
        {
            var response = await _userRepository.RegisterUser(newUser);

            Assert.IsTrue(response.Successful);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(_dbService.Context.Users.Count(user => user.Id == response.Data!.Id), Is.EqualTo(1));
        }

        [TestCaseSource(nameof(userWithExistingEmail))]
        public async Task RegisterUser_UserWithEmailExists_ResponseIsNotSuccessful(RegisterUserDto newUser)
        {
            _dbService.FillUsersTable();

            var response = await _userRepository.RegisterUser(newUser);

            Assert.IsFalse(response.Successful);
            Assert.That(response.Data, Is.Null);
            Assert.That(_dbService.Context.Users.Count(user => user.Email == newUser.Email), Is.EqualTo(1));

        }

        [TearDown]
        public void ClearUsersTable()
        {
            _dbService.RemoveAllUsers();
        }

        public static readonly RegisterUserDto[] usersToRegister =
        {
            new()
            {
                FirstName = "Ekler",
                LastName = "Nedavni",
                Email = "email4@gmail.com",
                Password = "rizyabesstyzya6",
                Role = Role.Admin
            },
        };

        public static readonly RegisterUserDto[] userWithExistingEmail =
        {
            new()
            {
                FirstName = "Julia",
                LastName = "Nedavnia",
                Email = "email1@gmail.com",
                Password = "param25",
                Role = Role.Admin
            },
        };
    }
}
