using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using PLinkage.Models;
using PLinkage.Interfaces;
using CommunityToolkit.Mvvm.Input;
namespace PLinkage.ViewModels
{
    public partial class BrowseSkillProviderViewModel : ObservableObject
    {
        [ObservableProperty]
        private CebuLocation? selectedLocation = null;

        public ObservableCollection<CebuLocation> CebuLocations { get; } = new(
            Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        // Services
        private readonly INavigationService _navigationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        [ObservableProperty] private ObservableCollection<SkillProvider> suggestedSkillProviders = new();

        public IAsyncRelayCommand LoadDashboardDataCommand { get; }

        private string sortSelection = "All";
        public string SortSelection
        {
            get => sortSelection;
            set
            {
                if (SetProperty(ref sortSelection, value))
                    _ = LoadSuggestedSkillProviders();
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


        // Constructor
        public BrowseSkillProviderViewModel(INavigationService navigationService, IUnitOfWork unitOfWork, ISessionService sessionService)
        {
            _navigationService = navigationService;
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            LoadDashboardDataCommand = new AsyncRelayCommand(LoadDashboardData);
        }

        // Core Methods
        private async Task LoadDashboardData()
        {
            await _unitOfWork.ReloadAsync();
            var currentUser = _sessionService.GetCurrentUser();
            if (currentUser == null) return;

            await LoadSuggestedSkillProviders();
        }

        private async Task LoadSuggestedSkillProviders()
        {
            // fetch all and exclude deactivated users
            var skillProviders = (await _unitOfWork.SkillProvider.GetAllAsync())
                .Where(sp => !string.Equals(sp.UserStatus, "Deactivated", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var currentUser = await _unitOfWork.ProjectOwner
                .GetByIdAsync(_sessionService.GetCurrentUser().UserId);
            if (currentUser == null || !currentUser.UserLocation.HasValue)
                return;

            var ownerCoord = CebuLocationCoordinates.Map[currentUser.UserLocation.Value];

            IEnumerable<SkillProvider> filtered = SortSelection switch
            {
                "Same Place as Me" => skillProviders
                    .Where(sp => sp.UserLocation == currentUser.UserLocation),

                "Nearby (<=10km)" => skillProviders
                    .Where(sp =>
                        sp.UserLocation.HasValue &&
                        CebuLocationCoordinates.Map.ContainsKey(sp.UserLocation.Value) &&
                        CalculateDistanceKm(ownerCoord, CebuLocationCoordinates.Map[sp.UserLocation.Value]) <= 10),

                "Within Urban (<=30km)" => skillProviders
                    .Where(sp =>
                        sp.UserLocation.HasValue &&
                        CebuLocationCoordinates.Map.ContainsKey(sp.UserLocation.Value) &&
                        CalculateDistanceKm(ownerCoord, CebuLocationCoordinates.Map[sp.UserLocation.Value]) <= 30),

                "Extended (<=50km)" => skillProviders
                    .Where(sp =>
                        sp.UserLocation.HasValue &&
                        CebuLocationCoordinates.Map.ContainsKey(sp.UserLocation.Value) &&
                        CalculateDistanceKm(ownerCoord, CebuLocationCoordinates.Map[sp.UserLocation.Value]) <= 50),

                "By Specific Location" when SelectedLocation.HasValue => skillProviders
                    .Where(sp => sp.UserLocation == SelectedLocation),

                _ => skillProviders
            };



            SuggestedSkillProviders = new ObservableCollection<SkillProvider>(filtered);
        }

        partial void OnSelectedLocationChanged(CebuLocation? value)
        {
            if (SortSelection == "By Specific Location")
                _ = LoadSuggestedSkillProviders();
        }


        [RelayCommand]
        private async Task Refresh() => await LoadDashboardData();

        [RelayCommand]
        private async Task ViewSkillProvider(SkillProvider skillProvider)
        {
            _sessionService.VisitingSkillProviderID = skillProvider.UserId;
            await _navigationService.NavigateToAsync("/ViewSkillProviderProfileView");
        }

        private static double CalculateDistanceKm((double Latitude, double Longitude) coord1, (double Latitude, double Longitude) coord2)
        {
            const double EarthRadius = 6371; // km

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
