using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Models;

namespace ToDoApp.Interfaces
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        Task SignUpAsync(string username, string password);
        Task<bool> SignInAsync(string username, string password);
        void SignOut();
    }
}
