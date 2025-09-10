using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class AdminHomeViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        // Summary Stats
        [ObservableProperty] private int activeProjects;
        [ObservableProperty] private int completedProjects;
        [ObservableProperty] private string employmentRatio;

        // Collections
        [ObservableProperty] private ObservableCollection<Project> filteredProjects = new();
        [ObservableProperty] private ObservableCollection<SkillProvider> filteredSkillProviders = new();

        public ObservableCollection<string> ProjectStatusFilterOptions { get; } = new()
        {
            "All",
            "Active",
            "Completed",
            "Deactivated"
        };

        public ObservableCollection<string> SkillProviderFilterOptions { get; } = new()
        {
            "All",
            "Employed",
            "Unemployed"
        };

        public ObservableCollection<string> SkillProviderStatusFilterOptions { get; } = new()
        {
            "All",
            "Active",
            "Deactivated"
        };

        private string selectedProjectFilter = "All";
        public string SelectedProjectFilter
        {
            get => selectedProjectFilter;
            set
            {
                if (SetProperty(ref selectedProjectFilter, value))
                    _ = FilterProjects();
            }
        }

        private string selectedSkillEmploymentFilter = "All";
        public string SelectedSkillEmploymentFilter
        {
            get => selectedSkillEmploymentFilter;
            set
            {
                if (SetProperty(ref selectedSkillEmploymentFilter, value))
                    _ = FilterSkillProviders();
            }
        }

        private string selectedSkillStatusFilter = "All";
        public string SelectedSkillStatusFilter
        {
            get => selectedSkillStatusFilter;
            set
            {
                if (SetProperty(ref selectedSkillStatusFilter, value))
                    _ = FilterSkillProviders();
            }
        }

        public IAsyncRelayCommand LoadDashboardDataCommand { get; }

        public AdminHomeViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            _sessionService = sessionService;
            LoadDashboardDataCommand = new AsyncRelayCommand(LoadDashboardData);
        }

        private async Task LoadDashboardData()
        {
            await _unitOfWork.ReloadAsync();

            await CountProjects();
            await CalculateSkillProviderEmploymentRatio();
            await FilterProjects();
            await FilterSkillProviders();
        }

        private async Task CountProjects()
        {
            var allProjects = await _unitOfWork.Projects.GetAllAsync();
            ActiveProjects = allProjects.Count(p => p.ProjectStatus == ProjectStatus.Active);
            CompletedProjects = allProjects.Count(p => p.ProjectStatus == ProjectStatus.Completed);
        }

        private async Task CalculateSkillProviderEmploymentRatio()
        {
            var allSkillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            var allProjects = await _unitOfWork.Projects.GetAllAsync();

            // Filter out deactivated skill providers
            var activeSkillProviders = allSkillProviders
                .Where(sp => !sp.UserStatus.Equals("Deactivated", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int total = activeSkillProviders.Count;
            int employed = activeSkillProviders.Count(sp =>
                allProjects.Any(p =>
                    p.ProjectMembers.Any(m => m.MemberId == sp.UserId) &&
                    p.ProjectStatus == ProjectStatus.Active));

            int unemployed = total - employed;

            EmploymentRatio = total == 0
                ? "N/A"
                : $"{employed}/{total} employed ({(employed * 100.0 / total):0.##}%)";
        }
        private async Task FilterProjects()
        {
            var allProjects = await _unitOfWork.Projects.GetAllAsync();

            IEnumerable<Project> filtered = SelectedProjectFilter switch
            {
                "Active" => allProjects.Where(p => p.ProjectStatus == ProjectStatus.Active),
                "Completed" => allProjects.Where(p => p.ProjectStatus == ProjectStatus.Completed),
                "Deactivated" => allProjects.Where(p => p.ProjectStatus == ProjectStatus.Deactivated),
                _ => allProjects
            };

            FilteredProjects = new ObservableCollection<Project>(filtered);
        }

        private async Task FilterSkillProviders()
        {
            var allSkillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            var allProjects = await _unitOfWork.Projects.GetAllAsync();

            var employedIds = allProjects
                .Where(p => p.ProjectStatus == ProjectStatus.Active)
                .SelectMany(p => p.ProjectMembers)
                .Select(m => m.MemberId)
                .Distinct()
                .ToHashSet();

            IEnumerable<SkillProvider> filtered = allSkillProviders;

            if (SelectedSkillEmploymentFilter == "Employed")
                filtered = filtered.Where(sp => employedIds.Contains(sp.UserId));
            else if (SelectedSkillEmploymentFilter == "Unemployed")
                filtered = filtered.Where(sp => !employedIds.Contains(sp.UserId));

            if (SelectedSkillStatusFilter == "Active")
                filtered = filtered.Where(sp => !string.Equals(sp.UserStatus, "Deactivated", StringComparison.OrdinalIgnoreCase));
            else if (SelectedSkillStatusFilter == "Deactivated")
                filtered = filtered.Where(sp => string.Equals(sp.UserStatus, "Deactivated", StringComparison.OrdinalIgnoreCase));

            FilteredSkillProviders = new ObservableCollection<SkillProvider>(filtered);
        }

        [RelayCommand]
        private async Task Refresh() => await LoadDashboardData();
        [RelayCommand]
        private async Task ViewProject(Project project)
        {
            _sessionService.VisitingProjectID = project.ProjectId;
            await _navigationService.NavigateToAsync("/ViewProjectView");
        }
        [RelayCommand]
        private async Task ViewSkillProvider(SkillProvider skillProvider)
        {
            _sessionService.VisitingSkillProviderID = skillProvider.UserId;
            await _navigationService.NavigateToAsync("/ViewSkillProviderProfileView");
        }
    }
}
