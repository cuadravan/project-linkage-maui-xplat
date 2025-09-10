using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Models;
using PLinkage.Interfaces;
namespace PLinkage.ViewModels
{
    public partial class BrowseProjectViewModel : ObservableObject
    {
        [ObservableProperty]
        private CebuLocation? selectedLocation = null;

        public ObservableCollection<CebuLocation> CebuLocations { get; } = new(
            Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        private readonly INavigationService _navigationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<Project> suggestedProjects = new();

        public IAsyncRelayCommand LoadDashboardDataCommand { get; }

        private string sortSelection = "All";
        public string SortSelection
        {
            get => sortSelection;
            set
            {
                if (SetProperty(ref sortSelection, value))
                    _ = LoadSuggestedProjects();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new()
        {
            "All",
            "Same Place as Me",
            "Nearby (<=10km)",
            "Within Urban (<=30km)",
            "Extended (<=50km)",
            "By Specific Location"
        };


        public BrowseProjectViewModel(INavigationService navigationService, IUnitOfWork unitOfWork, ISessionService sessionService)
        {
            _navigationService = navigationService;
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            LoadDashboardDataCommand = new AsyncRelayCommand(LoadDashboardData);
        }

        private async Task LoadDashboardData()
        {
            await _unitOfWork.ReloadAsync();
            var currentUser = _sessionService.GetCurrentUser();
            if (currentUser == null) return;

            await LoadSuggestedProjects();
        }

        private async Task LoadSuggestedProjects()
        {
            var projects = (await _unitOfWork.Projects.GetAllAsync())
                .Where(p => p.ProjectStatus != ProjectStatus.Deactivated)
                .Where(p => p.ProjectStatus != ProjectStatus.Completed)
                .ToList();

            var currentUser = await _unitOfWork.SkillProvider
                .GetByIdAsync(_sessionService.GetCurrentUser().UserId);

            if (currentUser == null || !currentUser.UserLocation.HasValue)
                return;

            var userId = currentUser.UserId;

            projects = projects
                .Where(p => p.ProjectMembers == null || !p.ProjectMembers
                    .Any(m => m.MemberId == userId))
                .ToList();

            var userCoord = CebuLocationCoordinates.Map[currentUser.UserLocation.Value];

            IEnumerable<Project> filtered = SortSelection switch
            {
                "Same Place as Me" => projects
                    .Where(p => p.ProjectLocation == currentUser.UserLocation),

                "Nearby (<=10km)" => projects
                    .Where(p =>
                        p.ProjectLocation.HasValue &&
                        CebuLocationCoordinates.Map.ContainsKey(p.ProjectLocation.Value) &&
                        CalculateDistanceKm(userCoord, CebuLocationCoordinates.Map[p.ProjectLocation.Value]) <= 10),

                "Within Urban (<=30km)" => projects
                    .Where(p =>
                        p.ProjectLocation.HasValue &&
                        CebuLocationCoordinates.Map.ContainsKey(p.ProjectLocation.Value) &&
                        CalculateDistanceKm(userCoord, CebuLocationCoordinates.Map[p.ProjectLocation.Value]) <= 30),

                "Extended (<=50km)" => projects
                    .Where(p =>
                        p.ProjectLocation.HasValue &&
                        CebuLocationCoordinates.Map.ContainsKey(p.ProjectLocation.Value) &&
                        CalculateDistanceKm(userCoord, CebuLocationCoordinates.Map[p.ProjectLocation.Value]) <= 50),

                "By Specific Location" when SelectedLocation.HasValue => projects
                    .Where(p => p.ProjectLocation == SelectedLocation),

                _ => projects
            };


            SuggestedProjects = new ObservableCollection<Project>(filtered);
        }

        partial void OnSelectedLocationChanged(CebuLocation? value)
        {
            if (SortSelection == "By Specific Location")
                _ = LoadSuggestedProjects();
        }

        [RelayCommand]
        private async Task Refresh() => await LoadDashboardData();

        [RelayCommand]
        private async Task ViewProject(Project project)
        {
            _sessionService.VisitingProjectID = project.ProjectId;
            await _navigationService.NavigateToAsync("/ViewProjectView");
        }

        private static double CalculateDistanceKm((double Latitude, double Longitude) coord1, (double Latitude, double Longitude) coord2)
        {
            const double EarthRadius = 6371;

            double lat1Rad = Math.PI * coord1.Latitude / 180;
            double lat2Rad = Math.PI * coord2.Latitude / 180;
            double deltaLat = lat2Rad - lat1Rad;
            double deltaLon = Math.PI * (coord2.Longitude - coord1.Longitude) / 180;

            double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Pow(Math.Sin(deltaLon / 2), 2);

            return EarthRadius * (2 * Math.Asin(Math.Sqrt(a)));
        }
    }
}
