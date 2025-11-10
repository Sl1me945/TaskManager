namespace ToDoApp.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string hashed, string password);
    }
}
