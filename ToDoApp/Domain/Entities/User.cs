namespace ToDoApp.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public User(string username, string passwordHash) 
        {
            Id = Guid.NewGuid();
            Username = username;
            PasswordHash = passwordHash;
        }

        public User() { }
    }
}
