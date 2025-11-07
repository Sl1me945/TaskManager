
namespace ToDoApp.DTOs
{
    public class UserDto
    {
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public List<TaskDto> Tasks { get; set; } = new();
    }
}
