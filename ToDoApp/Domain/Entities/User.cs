using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Domain.Entities
{
    public class User
    {
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public List<BaseTask> Tasks { get; set; } = [];

        public User(string username, string passwordHash) 
        {
            Username = username;
            PasswordHash = passwordHash;
        }

        public User() { }
    }
}
