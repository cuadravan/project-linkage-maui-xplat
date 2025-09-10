using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Interfaces;
using PLinkage.Models;
using System.Globalization;

namespace PLinkage.ViewModels
{
    public partial class ViewProjectOwnerProfileViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        private Guid _projectOwnerId;

        // Properties
        [ObservableProperty] private string userName;
        [ObservableProperty] private string userLocation;
        [ObservableProperty] private DateTime dateJoined;
        [ObservableProperty] private string userGender;
        [ObservableProperty] private string userEmail;
        [ObservableProperty] private string userPhone;
        [ObservableProperty] private ObservableCollection<Project> ownedProjects = new();
        [ObservableProperty] private string toggleActivationButtonText;


        // Role Flags
        [ObservableProperty] private bool isSkillProvider;
        [ObservableProperty] private bool isAdmin;
        [ObservableProperty] private bool isSkillproviderOrAdmin;
        [ObservableProperty] private bool isOwner;

        public IAsyncRelayCommand OnViewAppearingCommand { get; }

        private string sortSelection = "All";
        public string SortSelection
        {
            get => sortSelection;
            set
            {
                if (SetProperty(ref sortSelection, value))
                    _ = LoadProjectsAsync();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new()
        {
            "Active",
            "Completed",
            "Deactivated",
            "All"
        };

        public ViewProjectOwnerProfileViewModel(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;
            OnViewAppearingCommand = new AsyncRelayCommand(OnViewAppearing);
        }

        public async Task OnViewAppearing()
        {
            SetRoleFlags();

            _projectOwnerId = _sessionService.VisitingProjectOwnerID;
            var currentUser = _sessionService.GetCurrentUser();
            if (_projectOwnerId == Guid.Empty && currentUser != null)
                _projectOwnerId = currentUser.UserId;

            IsOwner = currentUser != null && currentUser.UserId == _projectOwnerId;

            await _unitOfWork.ReloadAsync();

            // Load profile directly here to check status early
            var profile = await _unitOfWork.ProjectOwner.GetByIdAsync(_projectOwnerId);
            if (profile == null)
            {
                await Shell.Current.DisplayAlert("Error", "Project Owner not found.", "OK");
                await _navigationService.GoBackAsync();
                return;
            }

            // Check deactivation status and restrict access if not admin
            if (profile.UserStatus == "Deactivated" && !IsAdmin)
            {
                await Shell.Current.DisplayAlert("Access Denied", "This profile is deactivated.", "OK");
                await _navigationService.GoBackAsync();
                return;
            }

            // Now that status check passed, populate UI
            await LoadProfileAsync();
            await LoadProjectsAsync();

            if (IsOwner)
            {
                await Shell.Current.DisplayAlert("View Mode", "You are currently viewing your profile as a visitor.", "OK");
            }
        }


        private void SetRoleFlags()
        {
            var role = _sessionService.GetCurrentUserType();
            IsSkillProvider = role == UserRole.SkillProvider;
            IsAdmin = role == UserRole.Admin;
            IsSkillproviderOrAdmin = role == UserRole.SkillProvider || role == UserRole.Admin;
        }

        private async Task LoadProfileAsync()
        {
            var profile = await _unitOfWork.ProjectOwner.GetByIdAsync(_projectOwnerId);
            if (profile == null) return;

            UserName = $"{profile.UserFirstName} {profile.UserLastName}";
            UserLocation = profile.UserLocation?.ToString() ?? "Not specified";
            DateJoined = profile.JoinedOn;
            UserGender = profile.UserGender;
            UserEmail = profile.UserEmail;
            UserPhone = profile.UserPhone;

            // Set the toggle button text
            ToggleActivationButtonText = profile.UserStatus == "Deactivated" ? "Activate" : "Deactivate";
        }


        private async Task LoadProjectsAsync()
        {
            var projects = await _unitOfWork.Projects.GetAllAsync();
            var owned = projects.Where(p => p.ProjectOwnerId == _projectOwnerId);

            OwnedProjects = SortSelection switch
            {
                "Active" => new ObservableCollection<Project>(owned.Where(p => p.ProjectStatus == ProjectStatus.Active)),
                "Completed" => new ObservableCollection<Project>(owned.Where(p => p.ProjectStatus == ProjectStatus.Completed)),
                "Deactivated" => new ObservableCollection<Project>(owned.Where(p => p.ProjectStatus == ProjectStatus.Deactivated)),
                _ => new ObservableCollection<Project>(owned)
            };
        }

        [RelayCommand]
        private async Task ViewProject(Project project)
        {
            _sessionService.VisitingProjectID = project.ProjectId;
            await _navigationService.NavigateToAsync("/ViewProjectView");
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            _sessionService.VisitingReceiverID = _projectOwnerId;
            await _navigationService.NavigateToAsync("/ProjectOwnerSendMessageView");
        }

        [RelayCommand]
        private async Task ToggleProjectOwnerActivation()
        {
            var owner = await _unitOfWork.ProjectOwner.GetByIdAsync(_projectOwnerId);
            if (owner == null) return;

            string action = owner.UserStatus == "Deactivated" ? "Activate" : "Deactivate";

            bool confirm = await Shell.Current.DisplayAlert(
                $"Confirm {action}",
                $"{action} Project Owner: {owner.UserFirstName} {owner.UserLastName}?",
                "Yes", "No");

            if (!confirm) return;

            owner.UserStatus = owner.UserStatus == "Deactivated" ? "Active" : "Deactivated";

            await _unitOfWork.ProjectOwner.UpdateAsync(owner);
            await _unitOfWork.SaveChangesAsync();
            await LoadProfileAsync(); // Updates UI including button text
        }

    }
}
