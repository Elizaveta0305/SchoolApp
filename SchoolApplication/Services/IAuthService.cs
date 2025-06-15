using SchoolApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateUser(string username, string password);
    }
}
