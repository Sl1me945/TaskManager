using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Models;

namespace TaskManager.Interfaces
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        Task SignUpAsync(string username, string password, bool? isAdmin = null);
        Task<bool> SignInAsync(string username, string password);
        void SignOut();
    }
}
