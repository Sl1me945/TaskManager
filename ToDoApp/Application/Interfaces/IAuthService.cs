namespace ToDoApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task SignUpAsync(string username, string password);
        Task<string?> SignInAsync(string username, string password);
        Task SignOutAsync(string? token);
    }
}
