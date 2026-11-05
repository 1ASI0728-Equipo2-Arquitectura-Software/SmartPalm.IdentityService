namespace SmartPalm.IdentityService.Domain.Users;

public class User
{
    private User() { }

    public User(string username, string passwordHash, string email, string fullName, UserRole role)
    {
        Username = Require(username, nameof(username), 50);
        PasswordHash = Require(passwordHash, nameof(passwordHash), 200);
        Email = Require(email, nameof(email), 254);
        FullName = Require(fullName, nameof(fullName), 160);
        Role = role;
        Status = UserStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public void UpdateProfile(string fullName, string email)
    {
        FullName = Require(fullName, nameof(fullName), 160);
        Email = Require(email, nameof(email), 254);
    }

    public void RevokeAccess() => Status = UserStatus.Inactive;
    public void ActivateAccess() => Status = UserStatus.Active;

    private static string Require(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} cannot be empty.", name);
        if (value.Length > maxLength)
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", name);
        return value.Trim();
    }
}
