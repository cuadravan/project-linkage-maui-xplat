using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Models;
using PLinkage.Interfaces;

namespace PLinkage.ViewModels
{
    public partial class AdminBrowseProjectOwnerViewModel : ObservableObject
    {
        [ObservableProperty]
        private CebuLocation? selectedLocation = null;

        public ObservableCollection<CebuLocation> CebuLocations { get; } = new(
            Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        private readonly INavigationService _navigationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<ProjectOwner> filteredProjectOwners = new();

        public ObservableCollection<string> CategorySortOptions { get; } = new()
        {
            "All",
            "By Specific Location"
        };

        public ObservableCollection<string> StatusSortOptions { get; } = new()
        {
            "All",
            "Active Only",
            "Deactivated Only"
        };

        private string categorySortSelection = "All";
        public string CategorySortSelection
        {
            get => categorySortSelection;
            set
            {
                if (SetProperty(ref categorySortSelection, value))
                    _ = LoadProjectOwners();
            }
        }

        private string statusSortSelection = "All";
        public string StatusSortSelection
        {
            get => statusSortSelection;
            set
            {
                if (SetProperty(ref statusSortSelection, value))
                    _ = LoadProjectOwners();
            }
        }

        public IAsyncRelayCommand LoadDashboardDataCommand { get; }

        public AdminBrowseProjectOwnerViewModel(INavigationService navigationService, IUnitOfWork unitOfWork, ISessionService sessionService)
        {
            _navigationService = navigationService;
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            LoadDashboardDataCommand = new AsyncRelayCommand(LoadDashboardData);
        }

        private async Task LoadDashboardData()
        {
            await _unitOfWork.ReloadAsync();
            await LoadProjectOwners();
        }

        private async Task LoadProjectOwners()
        {
            var projectOwners = await _unitOfWork.ProjectOwner.GetAllAsync();
            var projects = await _unitOfWork.Projects.GetAllAsync();

            IEnumerable<ProjectOwner> filtered = projectOwners;

            // Apply User Status Filter
            filtered = StatusSortSelection switch
            {
                "Active Only" => filtered.Where(sp => !string.Equals(sp.UserStatus, "Deactivated", StringComparison.OrdinalIgnoreCase)),
                "Deactivated Only" => filtered.Where(sp => string.Equals(sp.UserStatus, "Deactivated", StringComparison.OrdinalIgnoreCase)),
                _ => filtered
            };

            // Apply Category Filter
            filtered = CategorySortSelection switch
            {

                "By Specific Location" when SelectedLocation.HasValue =>
                    filtered.Where(sp => sp.UserLocation == SelectedLocation),

                _ => filtered
            };

            FilteredProjectOwners = new ObservableCollection<ProjectOwner>(filtered);
        }

        partial void OnSelectedLocationChanged(CebuLocation? value)
        {
            if (CategorySortSelection == "By Specific Location")
                _ = LoadProjectOwners();
        }

        [RelayCommand]
        private async Task Refresh() => await LoadDashboardData();

        [RelayCommand]
        private async Task ViewProjectOwner(ProjectOwner projectOwner)
        {
            _sessionService.VisitingProjectOwnerID = projectOwner.UserId;
            await _navigationService.NavigateToAsync("/ViewProjectOwnerProfileView");
        }
    }
}
