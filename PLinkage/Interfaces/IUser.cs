using PLinkage.Models;

namespace PLinkage.Interfaces
{
    public interface IUser
    {
        Guid UserId { get; }
        string UserFirstName { get; }
        string UserLastName { get; }
        UserRole UserRole { get; }
        CebuLocation? UserLocation { get; }
    }
}

public enum UserRole
{
    SkillProvider,
    ProjectOwner,
    Admin
}
