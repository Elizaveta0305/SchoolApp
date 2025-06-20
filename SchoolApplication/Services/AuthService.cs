using SchoolApplication.Data;
using SchoolApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Diagnostics;

namespace SchoolApplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public AuthService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<User?> AuthenticateUser(string username, string password)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var user = await context.Users
                                         .Include(u => u.Role)
                                         .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null || user.PasswordHash == null)
                {
                    return null;
                }

                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return user;
                }

                return null;
            }
        }
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> RegisterUserInternal(string username, string password, string roleName, int? groupId = null, string? firstName = null, string? middleName = null, string? lastName = null)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                if (await context.Users.AnyAsync(u => u.Username == username))
                {
                    return false;
                }

                var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                {
                    return false;
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = hashedPassword,
                    RoleID = role.RoleID,
                    Role = role,
                    GroupID = groupId,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName
                };

                context.Users.Add(newUser);
                await context.SaveChangesAsync();
                return true;
            }
        }
    }
}