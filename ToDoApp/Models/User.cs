using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Interfaces;

namespace ToDoApp.Models
{
    public class User
    {
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public List<ITask> Tasks { get; set; } = new List<ITask>();

        public User(string username, string passwordHash) 
        {
            Username = username;
            PasswordHash = passwordHash;
        }

        public User() { }
    }
}
