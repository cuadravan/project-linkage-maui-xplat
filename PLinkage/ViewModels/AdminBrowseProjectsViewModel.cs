using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Models;
using PLinkage.Interfaces;

namespace PLinkage.ViewModels
{
    public partial class AdminBrowseProjectsViewModel : ObservableObject
    {
        [ObservableProperty]
        private CebuLocation? selectedLocation = null;

        public ObservableCollection<CebuLocation> CebuLocations { get; } = new(
            Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        private readonly INavigationService _navigationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<Project> allProjects = new();

        public IAsyncRelayCommand LoadProjectsCommand { get; }

        private string sortSelection = "All";
        public string SortSelection
        {
            get => sortSelection;
            set
            {
                if (SetProperty(ref sortSelection, value))
                    _ = LoadFilteredProjects();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new()
        {
            "All",
            "By Specific Location",
            "Active Only",
            "Completed Only",
            "Deactivated Only"
        };

        public AdminBrowseProjectsViewModel(INavigationService navigationService, IUnitOfWork unitOfWork, ISessionService sessionService)
        {
            _navigationService = navigationService;
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            LoadProjectsCommand = new AsyncRelayCommand(LoadProjects);
        }

        private async Task LoadProjects()
        {
            await _unitOfWork.ReloadAsync();
            await LoadFilteredProjects();
        }

        private async Task LoadFilteredProjects()
        {
            var projects = await _unitOfWork.Projects.GetAllAsync();

            IEnumerable<Project> filtered = SortSelection switch
            {
                "By Specific Location" when SelectedLocation.HasValue => projects
                    .Where(p => p.ProjectLocation == SelectedLocation),

                "Active Only" => projects
                    .Where(p => p.ProjectStatus == ProjectStatus.Active),

                "Completed Only" => projects
                    .Where(p => p.ProjectStatus == ProjectStatus.Completed),

                "Deactivated Only" => projects
                    .Where(p => p.ProjectStatus == ProjectStatus.Deactivated),

                _ => projects
            };

            AllProjects = new ObservableCollection<Project>(filtered);
        }

        partial void OnSelectedLocationChanged(CebuLocation? value)
        {
            if (SortSelection == "By Specific Location")
                _ = LoadFilteredProjects();
        }

        [RelayCommand]
        private async Task Refresh() => await LoadProjects();

        [RelayCommand]
        private async Task ViewProject(Project project)
        {
            _sessionService.VisitingProjectID = project.ProjectId;
            await _navigationService.NavigateToAsync("/ViewProjectView");
        }
    }
}
