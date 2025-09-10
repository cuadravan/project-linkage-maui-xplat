using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class ProjectOwnerProfileViewModel : ObservableObject
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

        // Constructor
        public ProjectOwnerProfileViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;
            OnViewAppearingCommand = new AsyncRelayCommand(OnViewAppearing);
        }

        // Core Methods
        public async Task OnViewAppearing()
        {
            _projectOwnerId = _sessionService.GetCurrentUser().UserId;

            await _unitOfWork.ReloadAsync();
            await LoadProfileAsync();
            await LoadProjectsAsync();
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

        // Commands
        [RelayCommand]
        private async Task UpdateProfile() => await _navigationService.NavigateToAsync("/ProjectOwnerUpdateProfileView");

        [RelayCommand]
        private async Task AddProject() => await _navigationService.NavigateToAsync("/ProjectOwnerAddProjectView");

        [RelayCommand]
        private async Task ViewProject(Project project)
        {
            _sessionService.VisitingProjectID = project.ProjectId;
            await _navigationService.NavigateToAsync("/ViewProjectView");
        }

        [RelayCommand]
        private async Task UpdateProject(Project project)
        {
            if (project.ProjectStatus == ProjectStatus.Completed)
            {
                await Shell.Current.DisplayAlert("⚠️ Error", "You cannot update a completed project.", "OK");
                return;
            }
            _sessionService.VisitingProjectID = project.ProjectId;
            await _navigationService.NavigateToAsync("/ProjectOwnerUpdateProjectView");
        }
    }
}
