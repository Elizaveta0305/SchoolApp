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

        [Fact]
        public async Task AuthenticateUser_ReturnsNull_WhenUserDoesNotExist()
        {
            var dbContextFactory = CreateInMemoryDbContextFactory();
            using (var context = dbContextFactory.CreateDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
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
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var studentRole = new Role { RoleID = 1, RoleName = "Student" };
                context.Roles.Add(studentRole);
                await context.SaveChangesAsync();

                string correctPassword = "Password123";
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

                context.Users.Add(new User
                {
                    UserID = 1,
                    Username = "testuser",
                    PasswordHash = hashedPassword,
                    RoleID = studentRole.RoleID,
                    FirstName = "Test",
                    LastName = "User"
                });
                await context.SaveChangesAsync();
            }

            var authService = new AuthService(dbContextFactory);

            string username = "testuser";
            string incorrectPassword = "Password";

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
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var adminRole = new Role { RoleID = 1, RoleName = "Admin" };
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(testPassword);

                var testUser = new User
                {
                    UserID = 1,
                    Username = testUsername,
                    PasswordHash = hashedPassword,
                    RoleID = adminRole.RoleID,
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
    }
}