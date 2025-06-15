using SchoolApplication.Data;
using SchoolApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace SchoolApplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateUser(string username, string password)
        {
            var user = await _context.Users
                                     .Include(u => u.Role)
                                     .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            if (password == user.PasswordHash)
                                               // if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<bool> RegisterUser(string username, string password, string roleName)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return false;
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null)
            {
                return false;
            }

            // string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password); // Закомментируйте эту строку

            var newUser = new User
            {
                Username = username,
                PasswordHash = password,
                RoleID = role.RoleID,
                Role = role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
