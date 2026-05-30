using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Domain.Entities
{
    public sealed class User
    {
        private const int MinUsernameLength = 6;
        private const int MaxUsernameLength = 20;

        private static readonly Regex Pbkdf2 =
            new(@"^pbkdf2\$\d+\$[A-Za-z0-9+/=]+\$[A-Za-z0-9+/=]+$",
                RegexOptions.Compiled);

        private string _username = "";
        private string _passwordHash = "";
        private readonly List<BaseTask> _tasks = new();

        public Guid Id { get; init; } = Guid.NewGuid();

        public string Username 
        {
            get => _username;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Username cannot be empty");

                if (value.Length < MinUsernameLength
                    || value.Length > MaxUsernameLength)
                    throw new ArgumentException("Invalid length");

                _username = value;
            }
        }

        public string PasswordHash
        {
            get => _passwordHash;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Password hash cannot be empty");

                if (!Pbkdf2.IsMatch(value))
                    throw new ArgumentException("Invalid password hash format");

                _passwordHash = value;
            }
        }

        public IReadOnlyCollection<BaseTask> Tasks => _tasks.AsReadOnly();

        public User(string username, string passwordHash)
        {
            Username = username;
            PasswordHash = passwordHash;
        }

        internal User(Guid id, string username, string passwordHash)
            : this(username, passwordHash)
        {
            Id = id;
        }

        public void UpdateUsername(string username) => Username = username;
        public void UpdatePasswordHash(string hash) => PasswordHash = hash;
    }
}
