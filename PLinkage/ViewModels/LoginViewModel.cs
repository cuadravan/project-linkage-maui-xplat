using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLinkage.Interfaces;

namespace PLinkage.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        // Services
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly AppShellViewModel _appShellViewModel;
        private readonly ISessionService _sessionService;

        // Constructor
        public LoginViewModel(
            INavigationService navigationService,
            IAuthenticationService authenticationService,
            AppShellViewModel appShellViewModel,
            ISessionService sessionService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _appShellViewModel = appShellViewModel;
            _sessionService = sessionService;
        }

        // Properties
        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private string errorMessage;

        // Commands
        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email and password.";
                return;
            }

            var user = await _authenticationService.LoginAsync(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                return;
            }

            _sessionService.SetCurrentUser(user);
            _appShellViewModel.UpdateRoleProperties();
            ErrorMessage = string.Empty;

            switch (_sessionService.GetCurrentUserType())
            {
                case UserRole.SkillProvider:
                    await _navigationService.NavigateToAsync("SkillProviderHomeView");
                    break;
                case UserRole.ProjectOwner:
                    await _navigationService.NavigateToAsync("ProjectOwnerHomeView");
                    break;
                case UserRole.Admin:
                    await _navigationService.NavigateToAsync("AdminHomeView");
                    break;
                default:
                    ErrorMessage = "Unknown user role.";
                    break;
            }
        }

        [RelayCommand]
        private async Task GoToRegister() => await _navigationService.NavigateToAsync("/RegisterView");
    }
}
