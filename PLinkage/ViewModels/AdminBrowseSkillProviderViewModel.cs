using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Models;
using PLinkage.Interfaces;

namespace PLinkage.ViewModels
{
    public partial class AdminBrowseSkillProviderViewModel : ObservableObject
    {
        [ObservableProperty]
        private CebuLocation? selectedLocation = null;

        public ObservableCollection<CebuLocation> CebuLocations { get; } = new(
            Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        private readonly INavigationService _navigationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<SkillProvider> filteredSkillProviders = new();

        public ObservableCollection<string> CategorySortOptions { get; } = new()
        {
            "All",
            "By Specific Location",
            "Employed Only",
            "Unemployed Only"
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
                    _ = LoadSkillProviders();
            }
        }

        private string statusSortSelection = "All";
        public string StatusSortSelection
        {
            get => statusSortSelection;
            set
            {
                if (SetProperty(ref statusSortSelection, value))
                    _ = LoadSkillProviders();
            }
        }

        public IAsyncRelayCommand LoadDashboardDataCommand { get; }

        public AdminBrowseSkillProviderViewModel(INavigationService navigationService, IUnitOfWork unitOfWork, ISessionService sessionService)
        {
            _navigationService = navigationService;
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            LoadDashboardDataCommand = new AsyncRelayCommand(LoadDashboardData);
        }

        private async Task LoadDashboardData()
        {
            await _unitOfWork.ReloadAsync();
            await LoadSkillProviders();
        }

        private async Task LoadSkillProviders()
        {
            var skillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            var projects = await _unitOfWork.Projects.GetAllAsync();

            IEnumerable<SkillProvider> filtered = skillProviders;

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
                "Employed Only" => filtered.Where(sp =>
                    projects.Any(p => p.ProjectMembers.Any(m => m.MemberId == sp.UserId) && p.ProjectStatus == ProjectStatus.Active)),

                "Unemployed Only" => filtered.Where(sp =>
                    !projects.Any(p => p.ProjectMembers.Any(m => m.MemberId == sp.UserId) && p.ProjectStatus == ProjectStatus.Active)),

                "By Specific Location" when SelectedLocation.HasValue =>
                    filtered.Where(sp => sp.UserLocation == SelectedLocation),

                _ => filtered
            };

            FilteredSkillProviders = new ObservableCollection<SkillProvider>(filtered);
        }

        partial void OnSelectedLocationChanged(CebuLocation? value)
        {
            if (CategorySortSelection == "By Specific Location")
                _ = LoadSkillProviders();
        }

        [RelayCommand]
        private async Task Refresh() => await LoadDashboardData();

        [RelayCommand]
        private async Task ViewSkillProvider(SkillProvider skillProvider)
        {
            _sessionService.VisitingSkillProviderID = skillProvider.UserId;
            await _navigationService.NavigateToAsync("/ViewSkillProviderProfileView");
        }
    }
}
