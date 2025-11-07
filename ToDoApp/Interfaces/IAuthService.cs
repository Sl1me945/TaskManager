
namespace ToDoApp.Interfaces
{
    public interface IAuthService
    {
        Task SignUpAsync(string username, string password);
        Task<string?> SignInAsync(string username, string password);
        void SignOut();
    }
}
