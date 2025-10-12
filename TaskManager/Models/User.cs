using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Interfaces;

namespace TaskManager.Models
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; init; }
        public List<ITask> Tasks { get; set; } = new List<ITask>();

        public User(string username, string passwordHash, bool isAdmin) 
        {
            Username = username;
            PasswordHash = passwordHash;
            IsAdmin = isAdmin;
        }
    }
}
