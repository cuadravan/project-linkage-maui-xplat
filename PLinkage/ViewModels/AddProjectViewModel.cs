using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLinkage.Interfaces;
using PLinkage.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace PLinkage.ViewModels
{
    public partial class AddProjectViewModel : ObservableValidator
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        // Constructor
        public AddProjectViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;

            ProjectDateCreated = ProjectDateUpdated = DateTime.Now;
        }

        // Form fields
        [ObservableProperty] private CebuLocation? projectLocationSelected;
        [ObservableProperty, Required(ErrorMessage = "Project name is required.")] private string projectName;
        [ObservableProperty, Required(ErrorMessage = "Project description is required.")] private string projectDescription;
        [ObservableProperty] private DateTime projectStartDate = DateTime.Now;
        [ObservableProperty] private DateTime projectEndDate = DateTime.Now;
        [ObservableProperty, Required(ErrorMessage = "Project status is required.")] private ProjectStatus? projectStatusSelected;
        [ObservableProperty] private ObservableCollection<string> projectSkillsRequired = new();
        [ObservableProperty] private List<ProjectMemberDetail> projectMemberDetails = new();
        [ObservableProperty, Required(ErrorMessage = "Priority is required.")] private string projectPrioritySelected;
        [ObservableProperty, Range(1, int.MaxValue, ErrorMessage = "Resources needed must be at least 1.")] private int projectResourcesNeeded;
        [ObservableProperty] private DateTime projectDateCreated;
        [ObservableProperty] private DateTime projectDateUpdated;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private string durationSummary;
        [ObservableProperty] private string selectedSkill;

        public ObservableCollection<ProjectStatus> StatusOptions { get; } =
        new(Enum.GetValues(typeof(ProjectStatus)).Cast<ProjectStatus>().Where(x => x != ProjectStatus.Completed));

        public ObservableCollection<string> PriorityOptions { get; } = new() { "Low", "Medium", "High" };

        public ObservableCollection<CebuLocation> CebuLocations { get; } =
            new(Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        // Auto-update duration summary
        partial void OnProjectStartDateChanged(DateTime value) => UpdateDurationSummary();
        partial void OnProjectEndDateChanged(DateTime value) => UpdateDurationSummary();

        private void UpdateDurationSummary()
        {
            if (ProjectEndDate.Date < ProjectStartDate.Date)
            {
                DurationSummary = "Invalid date range";
                return;
            }

            var duration = ProjectEndDate - ProjectStartDate;
            DurationSummary = $"{(int)duration.TotalDays} days | {Math.Floor(duration.TotalDays / 7)} weeks | {Math.Floor(duration.TotalDays / 30)} months";
        }

        private bool ValidateForm()
        {
            ErrorMessage = string.Empty;
            ValidateAllProperties();

            if (HasErrors)
            {
                ErrorMessage = GetErrors()
                    .OfType<ValidationResult>()
                    .FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ProjectName) ||
                string.IsNullOrWhiteSpace(ProjectDescription) ||
                string.IsNullOrWhiteSpace(ProjectPrioritySelected))
            {
                ErrorMessage = "Please ensure all required fields are correctly filled.";
                return false;
            }

            if (!ProjectLocationSelected.HasValue)
            {
                ErrorMessage = "Project location must be selected.";
                return false;
            }

            if (ProjectStartDate.Date == default || ProjectEndDate.Date == default)
            {
                ErrorMessage = "Project start and end dates must be set.";
                return false;
            }

            if (ProjectEndDate.Date < ProjectStartDate.Date)
            {
                ErrorMessage = "Project end date cannot be earlier than start date.";
                return false;
            }

            if (ProjectStartDate.Date == ProjectEndDate.Date)
            {
                ErrorMessage = "Project start date cannot be the same as the end date.";
                return false;
            }

            if (ProjectSkillsRequired.Count == 0)
            {
                ErrorMessage = "At least one skill is required for the project.";
                return false;
            }

            if (ProjectStatusSelected == ProjectStatus.Completed)
            {
                ErrorMessage = "Project cannot be created with status Completed.";
                return false;
            }

            return true;
        }



        [RelayCommand]
        private async Task Submit()
        {
            if (!ValidateForm())
                return;

            var newProject = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectOwnerId = _sessionService.GetCurrentUser().UserId,
                ProjectName = ProjectName,
                ProjectLocation = ProjectLocationSelected,
                ProjectDescription = ProjectDescription,
                ProjectStartDate = ProjectStartDate,
                ProjectEndDate = ProjectEndDate,
                ProjectStatus = ProjectStatusSelected,
                ProjectSkillsRequired = ProjectSkillsRequired.ToList(),
                ProjectMembers = projectMemberDetails,
                ProjectPriority = ProjectPrioritySelected,
                ProjectResourcesNeeded = ProjectResourcesNeeded,
                ProjectResourcesAvailable = ProjectResourcesNeeded,
                ProjectDateCreated = ProjectDateCreated,
                ProjectDateUpdated = ProjectDateUpdated
            };

            await _unitOfWork.Projects.AddAsync(newProject);
            await _unitOfWork.SaveChangesAsync();

            await Shell.Current.DisplayAlert("Success", "Project created successfully!", "OK");
            await _navigationService.GoBackAsync();
        }

        [RelayCommand]
        private async Task Cancel() => await _navigationService.GoBackAsync();

        [RelayCommand]
        private void AddSkill()
        {
            var skill = SelectedSkill?.Trim();
            if (!string.IsNullOrWhiteSpace(skill) && !ProjectSkillsRequired.Contains(skill))
            {
                ProjectSkillsRequired.Add(skill);
                SelectedSkill = string.Empty;
            }
        }

        [RelayCommand]
        private void RemoveSkill(string skill)
        {
            if (ProjectSkillsRequired.Contains(skill))
                ProjectSkillsRequired.Remove(skill);
        }
    }
}
