using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLinkage.Models;
using PLinkage.Interfaces;
using System.Globalization;

namespace PLinkage.ViewModels
{
    public partial class ViewProjectViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        private Guid _projectId;
        private Guid _projectOwnerId;

        // Constructor
        public ViewProjectViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;

            OnAppearingCommand = new AsyncRelayCommand(OnAppearing);
        }

        // Properties
        [ObservableProperty] private string projectName;
        [ObservableProperty] private CebuLocation? projectLocation;
        [ObservableProperty] private string projectDescription;
        [ObservableProperty] private DateTime projectStartDate;
        [ObservableProperty] private DateTime projectEndDate;
        [ObservableProperty] private string projectPriority;
        [ObservableProperty] private ProjectStatus? currentProjectStatus;
        [ObservableProperty] private ObservableCollection<string> projectSkillsRequired = new();
        [ObservableProperty] private List<ProjectMemberDetail> projectMembers = new();
        [ObservableProperty] private int projectResourcesNeeded;
        [ObservableProperty] private DateTime projectDateCreated;
        [ObservableProperty] private DateTime projectDateUpdated;
        [ObservableProperty] private string durationSummary;
        [ObservableProperty] private ObservableCollection<EmployedSkillProviderWrapper> employedSkillProviders = new();
        [ObservableProperty] private bool isSkillProvider;
        [ObservableProperty] private bool isSkillproviderOrAdmin;
        [ObservableProperty] private bool isOwner;
        [ObservableProperty] private string projectOwnerFullName;


        public IAsyncRelayCommand OnAppearingCommand { get; }

        // Core logic
        public async Task OnAppearing()
        {
            _projectId = _sessionService.VisitingProjectID;
            if (_projectId == Guid.Empty) return;

            await _unitOfWork.ReloadAsync();
            // Fast role check using enum

            IsSkillProvider = _sessionService.GetCurrentUserType() == UserRole.SkillProvider;
            IsSkillproviderOrAdmin = _sessionService.GetCurrentUserType() == UserRole.SkillProvider ||
                                      _sessionService.GetCurrentUserType() == UserRole.Admin;

            await LoadProjectDetailsAsync();

            IsOwner = _sessionService.GetCurrentUser()?.UserId == _projectOwnerId;

            var duration = ProjectEndDate - ProjectStartDate;
            DurationSummary = $"{(int)duration.TotalDays} days | {Math.Floor(duration.TotalDays / 7)} weeks | {Math.Floor(duration.TotalDays / 30)} months";
        }


        private async Task LoadProjectDetailsAsync()
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(_projectId);
            if (project == null) return;

            _projectOwnerId = project.ProjectOwnerId;
            ProjectName = project.ProjectName;
            ProjectLocation = project.ProjectLocation;
            ProjectDescription = project.ProjectDescription;
            ProjectStartDate = project.ProjectStartDate;
            ProjectEndDate = project.ProjectEndDate;
            ProjectPriority = project.ProjectPriority;
            CurrentProjectStatus = project.ProjectStatus;
            ProjectSkillsRequired = new ObservableCollection<string>(project.ProjectSkillsRequired);
            ProjectMembers = project.ProjectMembers;
            ProjectResourcesNeeded = project.ProjectResourcesNeeded;
            ProjectDateCreated = project.ProjectDateCreated;
            ProjectDateUpdated = project.ProjectDateUpdated;

            // Fetch Project Owner's Full Name
            var owner = await _unitOfWork.ProjectOwner.GetByIdAsync(project.ProjectOwnerId);
            ProjectOwnerFullName = owner != null
                ? $"{owner.UserFirstName} {owner.UserLastName}"
                : "Unknown";

            UpdateDurationSummary();
            await LoadEmployedSkillProviders();
        }


        private async Task LoadEmployedSkillProviders()
        {
            var allSkillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            EmployedSkillProviders = new ObservableCollection<EmployedSkillProviderWrapper>(
        ProjectMembers.Select(pm =>
        {
            var sp = allSkillProviders.FirstOrDefault(s => s.UserId == pm.MemberId);
            return new EmployedSkillProviderWrapper
            {
                MemberId = pm.MemberId,
                FullName = sp != null ? $"{sp.UserFirstName} {sp.UserLastName}" : "Unknown",
                Email = sp?.UserEmail ?? "Unknown",
                Rate = pm.Rate,
                TimeFrame = pm.TimeFrame
            };
        }));
        }

        private void UpdateDurationSummary()
        {
            if (ProjectEndDate >= ProjectStartDate)
            {
                var duration = ProjectEndDate - ProjectStartDate;
                DurationSummary = $"{(int)duration.TotalDays} days | {Math.Floor(duration.TotalDays / 7)} weeks | {Math.Floor(duration.TotalDays / 30)} months";
            }
            else
            {
                DurationSummary = "Invalid date range";
            }
        }

        [RelayCommand]
        private async Task ViewSkillProvider(EmployedSkillProviderWrapper skillProvider)
        {
            _sessionService.VisitingSkillProviderID = skillProvider.MemberId;
            await _navigationService.NavigateToAsync("/ViewSkillProviderProfileView");
        }

        [RelayCommand]
        private async Task Apply()
        {
            var currentUser = _sessionService.GetCurrentUser();

            if (currentUser == null || currentUser.UserRole != UserRole.SkillProvider)
            {
                await Shell.Current.DisplayAlert("❗ Error", "Invalid user session. Please log in again.", "OK");
                return;
            }

            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(currentUser.UserId);

            if (skillProvider == null)
            {
                await Shell.Current.DisplayAlert("❗ Error", "Skill provider record not found.", "OK");
                return;
            }

            // Check if the project has any available slots ———
            var project = await _unitOfWork.Projects.GetByIdAsync(_projectId);
            if (project == null)
            {
                await Shell.Current.DisplayAlert("❗ Error", "Project not found.", "OK");
                return;
            }

            if (project.ProjectStatus != ProjectStatus.Active)
            {
                await Shell.Current.DisplayAlert(
                    "⚠️ Project is Currently Inactive or Completed",
                    "This project is not currently accepting applications.",
                    "OK");
                return;
            }

            if (project.ProjectResourcesAvailable <= 0)
            {
                await Shell.Current.DisplayAlert(
                    "⚠️ Project Full",
                    "This project has reached its maximum number of members and is no longer accepting applications.",
                    "OK");
                return;
            }

            if (skillProvider.EmployedProjects.Contains(_projectId))
            {
                await Shell.Current.DisplayAlert("⚠️ Already Employed", "You are already a member of this project.", "OK");
                return;
            }

            await _navigationService.NavigateToAsync("/SkillProviderSendApplicationView");
        }


        [RelayCommand]
        private async Task Back()
        {
            _sessionService.VisitingProjectID = Guid.Empty;
            await _navigationService.GoBackAsync();
        }

        [RelayCommand]
        private async Task ViewProjectOwner()
        {
            if (_projectOwnerId == Guid.Empty)
            {
                await Shell.Current.DisplayAlert("❗ Error", "Project owner not found.", "OK");
                return;
            }

            _sessionService.VisitingProjectOwnerID = _projectOwnerId;
            await _navigationService.NavigateToAsync("/ViewProjectOwnerProfileView");
        }

    }
}

public class EmployedSkillProviderWrapper
{
    public Guid MemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public int TimeFrame { get; set; }
}
