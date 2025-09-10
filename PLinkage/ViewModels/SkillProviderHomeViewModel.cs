using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class SkillProviderHomeViewModel : ObservableObject
    {
        // Services
        private readonly INavigationService _navigationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;


        // Properties
        [ObservableProperty] private string userName;
        [ObservableProperty] private ObservableCollection<Project> suggestedProjects = new();
        [ObservableProperty] private int receivedOfferCount;
        [ObservableProperty] private int sentApplicationCount;
        [ObservableProperty] private int activeProjects;
        [ObservableProperty] private string summaryText;

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
            "Extended (<=50km)"
        };


        // Constructor
        public SkillProviderHomeViewModel(INavigationService navigationService, IUnitOfWork unitOfWork, ISessionService sessionService)
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

            UserName = currentUser.UserFirstName ?? string.Empty;
            await LoadSuggestedProjects();
            await CountReceivedOffers(currentUser.UserId);
            await CountSentApplications(currentUser.UserId);
            await CountActiveProjects(currentUser.UserId);

            SummaryText = $"You have {ActiveProjects} active projects, {SentApplicationCount} pending sent applications, and {ReceivedOfferCount} received offers.";
        }

        private async Task LoadSuggestedProjects()
        {
            // fetch all and exclude deactivated and completed projects
            var projects = (await _unitOfWork.Projects.GetAllAsync())
                .Where(p => p.ProjectStatus != ProjectStatus.Deactivated &&
                            p.ProjectStatus != ProjectStatus.Completed)
                .ToList();

            var currentUser = await _unitOfWork.SkillProvider
                .GetByIdAsync(_sessionService.GetCurrentUser().UserId);

            if (currentUser == null || !currentUser.UserLocation.HasValue)
                return;

            var userId = currentUser.UserId;
            var userLocation = currentUser.UserLocation.Value;
            var userCoord = CebuLocationCoordinates.Map[userLocation];

            // Exclude projects where the current user is already employed
            projects = projects
                .Where(p => p.ProjectMembers == null || !p.ProjectMembers
                    .Any(m => m.MemberId == userId))
                .ToList();

            IEnumerable<Project> filtered = SortSelection switch
            {
                "Same Place as Me" => projects
                    .Where(p => p.ProjectLocation == userLocation),

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

                _ => projects
            };


            SuggestedProjects = new ObservableCollection<Project>(filtered);
        }



        private async Task CountReceivedOffers(Guid userId)
        {
            var allOffers = await _unitOfWork.OfferApplications.GetAllAsync();
            ReceivedOfferCount = allOffers.Count(offer => offer.ReceiverId == userId && offer.OfferApplicationStatus == "Pending");
        }

        private async Task CountSentApplications(Guid userId)
        {
            var allApplications = await _unitOfWork.OfferApplications.GetAllAsync();
            SentApplicationCount = allApplications.Count(app => app.SenderId == userId && app.OfferApplicationStatus == "Pending");
        }

        private async Task CountActiveProjects(Guid userId)
        {
            var allProjects = await _unitOfWork.Projects.GetAllAsync();
            ActiveProjects = allProjects.Count(
                p => p.ProjectMembers.Any(m => m.MemberId == userId)
                && p.ProjectStatus == ProjectStatus.Active
                );

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
