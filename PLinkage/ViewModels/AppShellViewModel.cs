using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLinkage.Interfaces;
using PLinkage.Views;

namespace PLinkage.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        public AppShellViewModel(ISessionService sessionService, INavigationService navigationService)
        {
            _sessionService = sessionService;
            _navigationService = navigationService;
            UpdateRoleProperties();
        }

        [ObservableProperty]
        private string welcomeMessage;

        [ObservableProperty]
        private bool isAdmin;

        [ObservableProperty]
        private bool isProjectOwner;

        [ObservableProperty]
        private bool isSkillProvider;

        [ObservableProperty]
        private bool isNotLoggedIn;

        [ObservableProperty]
        private bool isLoggedIn;

        [ObservableProperty]
        private string userRoleMessage;

        public void UpdateRoleProperties()
        {
            var role = _sessionService.GetCurrentUser()?.UserRole;

            IsAdmin = role == UserRole.Admin;
            IsProjectOwner = role == UserRole.ProjectOwner;
            IsSkillProvider = role == UserRole.SkillProvider;
            IsNotLoggedIn = role == null;
            IsLoggedIn = !IsNotLoggedIn;

            if (IsNotLoggedIn)
            {
                WelcomeMessage = "Welcome to PLinkage!";
                UserRoleMessage = string.Empty;
            }
            else
            {
                var user = _sessionService.GetCurrentUser();
                WelcomeMessage = $"Welcome to PLinkage, {user?.UserFirstName}!";
                UserRoleMessage = user?.UserRole.ToString();
            }

        }

        [RelayCommand]
        public async Task Logout()
        {
            _sessionService.ClearSession();
            UpdateRoleProperties();
            await _navigationService.NavigateToAsync(nameof(LoginView));
        }
    }
}
