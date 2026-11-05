using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.UnitTests;

public class UserTests
{
    [Fact]
    public void Constructor_CreatesActiveUserWithRequestedRole()
    {
        var user = new User("grower", "hashed-password", "grower@example.com", "Palm Grower", UserRole.PalmGrower);

        Assert.Equal(UserRole.PalmGrower, user.Role);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal("grower", user.Username);
    }

    [Fact]
    public void Constructor_RejectsEmptyUsername()
    {
        var action = () => new User(" ", "hashed-password", "grower@example.com", "Palm Grower", UserRole.PalmGrower);
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void RevokeAndActivateAccess_ChangeStatus()
    {
        var user = new User("grower", "hashed-password", "grower@example.com", "Palm Grower", UserRole.PalmGrower);
        user.RevokeAccess();
        Assert.Equal(UserStatus.Inactive, user.Status);
        user.ActivateAccess();
        Assert.Equal(UserStatus.Active, user.Status);
    }
}
