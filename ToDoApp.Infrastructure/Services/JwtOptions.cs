namespace ToDoApp.Infrastructure.Services
{
    public class JwtOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "ToDoApp";
        public string Audience { get; set; } = "ToDoAppClient";
        public int ExpiryHours { get; set; } = 1;
    }
}
