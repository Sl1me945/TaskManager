using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.DTOs
{
    public class UserDto
    {
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public bool IsAdmin { get; set; }
        public List<TaskDto> Tasks { get; set; } = new();
    }
}
