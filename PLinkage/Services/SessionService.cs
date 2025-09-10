using PLinkage.Interfaces;

namespace PLinkage.Services
{
    public class SessionService : ISessionService
    {
        private IUser? _currentUser;
        private Guid visitingProjectOwnerID = Guid.Empty;
        private Guid visitingSkillProviderID = Guid.Empty;
        private Guid visitingProjectID = Guid.Empty;
        private Guid visitingReceiverID = Guid.Empty;

        public Guid VisitingProjectOwnerID
        {
            get => visitingProjectOwnerID;
            set => visitingProjectOwnerID = value;
        }
        public Guid VisitingSkillProviderID
        {
            get => visitingSkillProviderID;
            set => visitingSkillProviderID = value;
        }
        public Guid VisitingProjectID
        {
            get => visitingProjectID;
            set => visitingProjectID = value;
        }

        public Guid VisitingReceiverID
        {
            get => visitingReceiverID;
            set => visitingReceiverID = value;
        }

        public void SetCurrentUser(IUser user)
        {
            _currentUser = user; // We need login view model now to set user to NULL then in appshell view model, if null, then set to IsNotLoggedIn
        }

        public IUser? GetCurrentUser()
        {
            return _currentUser;
        }

        public void ClearSession()
        {
            _currentUser = null;
        }

        public bool IsLoggedIn()
        {
            return _currentUser != null;
        }

        public UserRole? GetCurrentUserType()
        {
            return _currentUser?.UserRole;
        }

        public int VisitingSkillEducationID
        {
            get => visitingSkillEducationID;
            set => visitingSkillEducationID = value;
        }

        private int visitingSkillEducationID = 0;

    }
}
