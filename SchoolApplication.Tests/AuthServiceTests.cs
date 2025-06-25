using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolApplication.Data;
using SchoolApplication.Models;
using SchoolApplication.Services;
using Xunit;
using System.Threading.Tasks;
using BCrypt.Net;
using System;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;

namespace SchoolApplication.Tests
{
    public class AuthServiceTests
    {
        private static readonly InMemoryDatabaseRoot _inMemoryDatabaseRoot = new();

        private IDbContextFactory<ApplicationDbContext> CreateInMemoryDbContextFactory()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase", databaseRoot: _inMemoryDatabaseRoot)
                .Options;

            var mockFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            mockFactory.Setup(f => f.CreateDbContext()).Returns(() => new ApplicationDbContext(options));
            return mockFactory.Object;
        }

        private async Task ClearAndSeedDatabase(ApplicationDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.Roles.Add(new Role { RoleID = 1, RoleName = "Admin" });
            context.Roles.Add(new Role { RoleID = 2, RoleName = "Teacher" });
            context.Roles.Add(new Role { RoleID = 3, RoleName = "Student" });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task AuthenticateUser_ReturnsNull_WhenUserDoesNotExist()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                await ClearAndSeedDatabase(context);
            }

            var authService = new AuthService(dbContextFactory);

            string nonExistentUsername = "nonexistentuser";
            string password = "anypassword";

            var result = await authService.AuthenticateUser(nonExistentUsername, password);

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUser_ReturnsNull_WhenPasswordIsIncorrect()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                await ClearAndSeedDatabase(context);

                string correctPassword = "Password123";
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

                context.Users.Add(new User
                {
                    UserID = 1,
                    Username = "testuser",
                    PasswordHash = hashedPassword,
                    RoleID = 3,
                    FirstName = "Test",
                    LastName = "User"
                });
                await context.SaveChangesAsync();
            }

            var authService = new AuthService(dbContextFactory);

            string username = "testuser";
            string incorrectPassword = "WrongPassword";

            var result = await authService.AuthenticateUser(username, incorrectPassword);

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateUser_ReturnsUser_WhenCredentialsAreCorrect()
        {
            string testUsername = "adminuser";
            string testPassword = "AdminPassword123";

            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                await ClearAndSeedDatabase(context);

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(testPassword);

                var testUser = new User
                {
                    UserID = 1,
                    Username = testUsername,
                    PasswordHash = hashedPassword,
                    RoleID = 1, // Admin role
                    FirstName = "Admin",
                    LastName = "User"
                };
                context.Users.Add(testUser);
                await context.SaveChangesAsync();
            }

            var authService = new AuthService(dbContextFactory);

            var result = await authService.AuthenticateUser(testUsername, testPassword);

            Assert.NotNull(result);
            Assert.Equal(testUsername, result.Username);
            Assert.Equal(1, result.UserID);
            Assert.Equal("Admin", result.Role?.RoleName);
        }

        [Fact]
        public void HashPassword_ReturnsValidHash()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            var authService = new AuthService(dbContextFactory);

            string plainPassword = "MySecretPassword123";
            string hashedPassword = authService.HashPassword(plainPassword);

            Assert.False(string.IsNullOrEmpty(hashedPassword));
            Assert.True(BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword));
        }

        [Fact]
        public async Task RegisterUserInternal_ReturnsTrueAndAddsUser_WhenSuccessful()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                await ClearAndSeedDatabase(context);
            }

            var authService = new AuthService(dbContextFactory);

            string username = "newuser";
            string password = "NewPassword123";
            string roleName = "Student";
            string firstName = "New";
            string lastName = "User";

            var result = await authService.RegisterUserInternal(username, password, roleName, null, firstName, null, lastName);

            Assert.True(result);

            using (var context = dbContextFactory.CreateDbContext())
            {
                var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
                Assert.NotNull(user);
                Assert.Equal(username, user.Username);
                Assert.True(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
                Assert.Equal(roleName, user.Role.RoleName);
                Assert.Equal(firstName, user.FirstName);
                Assert.Equal(lastName, user.LastName);
            }
        }

        [Fact]
        public async Task RegisterUserInternal_ReturnsFalse_WhenUserAlreadyExists()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                await ClearAndSeedDatabase(context);
                string existingUsername = "existinguser";
                context.Users.Add(new User
                {
                    UserID = 1,
                    Username = existingUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"),
                    RoleID = 3,
                    FirstName = "Existing",
                    LastName = "User"
                });
                await context.SaveChangesAsync();
            }

            var authService = new AuthService(dbContextFactory);

            string username = "existinguser";
            string password = "AnyPassword";
            string roleName = "Student";

            var result = await authService.RegisterUserInternal(username, password, roleName, null, "First", "Middle", "Last");

            Assert.False(result);

            using (var context = dbContextFactory.CreateDbContext())
            {
                Assert.Equal(1, await context.Users.CountAsync());
            }
        }

        [Fact]
        public async Task RegisterUserInternal_ReturnsFalse_WhenRoleDoesNotExist()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                await ClearAndSeedDatabase(context);
            }

            var authService = new AuthService(dbContextFactory);

            string username = "user_no_role";
            string password = "Pass123";
            string nonExistentRoleName = "NonExistentRole";

            var result = await authService.RegisterUserInternal(username, password, nonExistentRoleName, null, "First", null, "Last");

            Assert.False(result);

            using (var context = dbContextFactory.CreateDbContext())
            {
                Assert.False(await context.Users.AnyAsync(u => u.Username == username));
            }
        }

        [Fact]
        public async Task RegisterUserInternal_AddsUserWithAllOptionalParameters()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                await ClearAndSeedDatabase(context);
                context.Groups.Add(new Group { GroupID = 1, GroupName = "Test Group" });
                await context.SaveChangesAsync();
            }

            var authService = new AuthService(dbContextFactory);

            string username = "fulluser";
            string password = "FullUserPass";
            string roleName = "Student";
            int groupId = 1;
            string firstName = "Full";
            string middleName = "Middle";
            string lastName = "Name";

            var result = await authService.RegisterUserInternal(username, password, roleName, groupId, firstName, middleName, lastName);

            Assert.True(result);

            using (var context = dbContextFactory.CreateDbContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
                Assert.NotNull(user);
                Assert.Equal(username, user.Username);
                Assert.Equal(groupId, user.GroupID);
                Assert.Equal(firstName, user.FirstName);
                Assert.Equal(middleName, user.MiddleName);
                Assert.Equal(lastName, user.LastName);
            }
        }
    }
}