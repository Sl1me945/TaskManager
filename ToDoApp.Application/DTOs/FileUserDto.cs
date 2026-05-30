namespace ToDoApp.Application.DTOs
{
    public class FileUserDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }
}
