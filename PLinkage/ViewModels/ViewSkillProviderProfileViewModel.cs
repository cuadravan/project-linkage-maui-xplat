using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Interfaces;
using PLinkage.Models;
using System.Globalization;

namespace PLinkage.ViewModels
{
    public partial class ViewSkillProviderProfileViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        private Guid _skillProviderId;

        // User Info
        [ObservableProperty] private string userName;
        [ObservableProperty] private string userLocation;
        [ObservableProperty] private DateTime dateJoined;
        [ObservableProperty] private string userGender;
        [ObservableProperty] private string userEmail;
        [ObservableProperty] private string userPhone;
        [ObservableProperty] private double userRating;
        [ObservableProperty] private string toggleActivationButtonText;


        // Role Flags
        [ObservableProperty] private bool isProjectOwner;
        [ObservableProperty] private bool isSkillProvider;
        [ObservableProperty] private bool isAdmin;
        [ObservableProperty] private bool isProjectOwnerOrAdmin;
        [ObservableProperty] private bool isOwner;


        // Data Collections
        [ObservableProperty] private ObservableCollection<Skill> skills = new();
        [ObservableProperty] private ObservableCollection<Education> educations = new();
        [ObservableProperty] private ObservableCollection<Project> employedProjects = new();

        public IAsyncRelayCommand OnViewAppearingCommand { get; }

        public ViewSkillProviderProfileViewModel(
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

            _skillProviderId = _sessionService.VisitingSkillProviderID;
            var currentUser = _sessionService.GetCurrentUser();
            if (_skillProviderId == Guid.Empty && currentUser != null)
                _skillProviderId = currentUser.UserId;

            IsOwner = currentUser != null && currentUser.UserId == _skillProviderId;

            await _unitOfWork.ReloadAsync();

            // Load profile and check if user is deactivated
            var profile = await _unitOfWork.SkillProvider.GetByIdAsync(_skillProviderId);
            if (profile == null)
            {
                await Shell.Current.DisplayAlert("Error", "Skill Provider not found.", "OK");
                await _navigationService.GoBackAsync();
                return;
            }

            // Check status and allow only admins to view deactivated profiles
            if (profile.UserStatus == "Deactivated" && !IsAdmin)
            {
                await Shell.Current.DisplayAlert("Access Denied", "This profile is deactivated.", "OK");
                await _navigationService.GoBackAsync();
                return;
            }

            await LoadProfileAsync();
            await LoadEmployedProjectsAsync();

            if (IsOwner)
            {
                await Shell.Current.DisplayAlert("View Mode", "You are currently viewing your profile as a visitor.", "OK");
            }
        }



        private void SetRoleFlags()
        {
            var role = _sessionService.GetCurrentUserType();
            IsProjectOwner = role == UserRole.ProjectOwner;
            IsSkillProvider = role == UserRole.SkillProvider;
            IsAdmin = role == UserRole.Admin;
            IsProjectOwnerOrAdmin = IsProjectOwner || IsAdmin;
        }

        private async Task LoadProfileAsync()
        {
            var profile = await _unitOfWork.SkillProvider.GetByIdAsync(_skillProviderId);
            if (profile == null) return;

            UserName = $"{profile.UserFirstName} {profile.UserLastName}";
            UserLocation = profile.UserLocation?.ToString() ?? "Not specified";
            DateJoined = profile.JoinedOn;
            UserGender = profile.UserGender;
            UserEmail = profile.UserEmail;
            UserPhone = profile.UserPhone;
            UserRating = profile.UserRating;

            Educations = new ObservableCollection<Education>(profile.Educations);
            Skills = new ObservableCollection<Skill>(profile.Skills);
            ToggleActivationButtonText = profile.UserStatus == "Deactivated" ? "Activate" : "Deactivate";
        }

        private async Task LoadEmployedProjectsAsync()
        {
            EmployedProjects.Clear();

            var allProjects = await _unitOfWork.Projects.GetAllAsync();
            var employedIn = allProjects.Where(p =>
                p.ProjectMembers.Any(m => m.MemberId == _skillProviderId));

            foreach (var project in employedIn)
            {
                EmployedProjects.Add(project);
            }
        }

        // Commands
        [RelayCommand]
        private async Task SendOffer()
        {
            _sessionService.VisitingSkillProviderID = _skillProviderId;
            await _navigationService.NavigateToAsync("/ProjectOwnerSendOfferView");
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            _sessionService.VisitingReceiverID = _skillProviderId;
            await _navigationService.NavigateToAsync("/ProjectOwnerSendMessageView");
        }

        [RelayCommand]
        private async Task ViewProject(Project project)
        {
            _sessionService.VisitingProjectID = project.ProjectId;
            await _navigationService.NavigateToAsync("/ViewProjectView");
        }

        [RelayCommand]
        private async Task ToggleSkillProviderActivation()
        {
            var owner = await _unitOfWork.SkillProvider.GetByIdAsync(_skillProviderId);
            if (owner == null) return;

            string action = owner.UserStatus == "Deactivated" ? "Activate" : "Deactivate";

            bool confirm = await Shell.Current.DisplayAlert(
                $"Confirm {action}",
                $"{action} Skill Provider: {owner.UserFirstName} {owner.UserLastName}?",
                "Yes", "No");

            if (!confirm) return;

            owner.UserStatus = owner.UserStatus == "Deactivated" ? "Active" : "Deactivated";

            await _unitOfWork.SkillProvider.UpdateAsync(owner);
            await _unitOfWork.SaveChangesAsync();
            await LoadProfileAsync(); // Updates UI including button text
        }
    }
}
