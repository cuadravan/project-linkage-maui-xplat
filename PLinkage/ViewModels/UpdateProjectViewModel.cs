using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PLinkage.Interfaces;
using PLinkage.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;

namespace PLinkage.ViewModels
{
    public partial class UpdateProjectViewModel : ObservableValidator
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;
        private Guid _projectId;

        // Original collections to track changes
        private List<ProjectMemberDetail> _originalProjectMembers = new();
        private List<SkillProvider> _originalSkillProviders = new();

        // Track pending removals
        private List<SkillProvider> _pendingRemovals = new();

        // Track pending rejections (resignation denied)
        private List<ProjectMemberDetail> _pendingRejections = new();


        // Constructor
        public UpdateProjectViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;

            OnAppearingCommand = new AsyncRelayCommand(OnAppearing);
        }

        // Form fields (existing properties remain the same)
        [ObservableProperty] private CebuLocation? projectLocationSelected;
        [ObservableProperty, Required(ErrorMessage = "Project name is required.")] private string projectName;
        [ObservableProperty, Required(ErrorMessage = "Project description is required.")] private string projectDescription;
        [ObservableProperty] private DateTime projectStartDate;
        [ObservableProperty] private DateTime projectEndDate;
        [ObservableProperty, Required(ErrorMessage = "Project status is required.")] private ProjectStatus? projectStatusSelected;
        [ObservableProperty] private ObservableCollection<string> projectSkillsRequired = new();
        [ObservableProperty] private ObservableCollection<ProjectMemberDetail> projectMembers = new();
        [ObservableProperty, Required(ErrorMessage = "Priority is required.")] private string projectPrioritySelected;
        [ObservableProperty, Range(1, int.MaxValue, ErrorMessage = "Resources needed must be at least 1.")] private int projectResourcesNeeded;
        [ObservableProperty] private DateTime projectDateCreated;
        [ObservableProperty] private DateTime projectDateUpdated;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private string durationSummary;
        [ObservableProperty] private string selectedSkill;
        [ObservableProperty] private ObservableCollection<SkillProvider> employedSkillProviders = new();

        public ObservableCollection<ProjectStatus> StatusOptions { get; } =
            new(Enum.GetValues(typeof(ProjectStatus)).Cast<ProjectStatus>());

        public ObservableCollection<string> PriorityOptions { get; } = new() { "Low", "Medium", "High" };

        public ObservableCollection<CebuLocation> CebuLocations { get; } =
            new(Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        // Auto-update duration summary
        partial void OnProjectStartDateChanged(DateTime value) => UpdateDurationSummary();
        partial void OnProjectEndDateChanged(DateTime value) => UpdateDurationSummary();

        private void UpdateDurationSummary()
        {
            if (ProjectEndDate < ProjectStartDate)
            {
                DurationSummary = "Invalid date range";
                return;
            }

            var duration = ProjectEndDate - ProjectStartDate;
            DurationSummary = $"{(int)duration.TotalDays} days | {Math.Floor(duration.TotalDays / 7)} weeks | {Math.Floor(duration.TotalDays / 30)} months";
        }

        public IAsyncRelayCommand OnAppearingCommand { get; }

        // Core Methods
        public async Task OnAppearing()
        {
            _projectId = _sessionService.VisitingProjectID;
            if (_projectId == Guid.Empty) return;

            await _unitOfWork.ReloadAsync();
            await LoadCurrentProject();

            // Store original state for reset functionality
            _originalProjectMembers = new List<ProjectMemberDetail>(ProjectMembers);
            _originalSkillProviders = new List<SkillProvider>(EmployedSkillProviders);
            _pendingRemovals.Clear();

            await ProcessResignationRequest();
        }

        private async Task LoadCurrentProject()
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(_sessionService.VisitingProjectID);
            if (project == null) return;

            ProjectName = project.ProjectName;
            ProjectDescription = project.ProjectDescription;
            ProjectStartDate = project.ProjectStartDate;
            ProjectLocationSelected = project.ProjectLocation;
            ProjectStartDate = project.ProjectStartDate;
            ProjectEndDate = project.ProjectEndDate;
            ProjectPrioritySelected = project.ProjectPriority;
            ProjectStatusSelected = project.ProjectStatus;
            ProjectSkillsRequired = new ObservableCollection<string>(project.ProjectSkillsRequired);
            ProjectMembers = new ObservableCollection<ProjectMemberDetail>(project.ProjectMembers);
            ProjectResourcesNeeded = project.ProjectResourcesNeeded;
            ProjectDateCreated = project.ProjectDateCreated;
            ProjectDateUpdated = project.ProjectDateUpdated;
            await LoadEmployedSkillProviders();
            UpdateDurationSummary();
        }

        private async Task LoadEmployedSkillProviders()
        {
            var allSkillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            var memberIds = ProjectMembers.Select(pm => pm.MemberId);
            var filtered = allSkillProviders.Where(sp => memberIds.Contains(sp.UserId));
            EmployedSkillProviders = new ObservableCollection<SkillProvider>(filtered);
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
                !ProjectLocationSelected.HasValue ||
                string.IsNullOrWhiteSpace(ProjectPrioritySelected) ||
                ProjectEndDate < ProjectStartDate)
            {
                ErrorMessage = "Please ensure all required fields are correctly filled.";
                return false;
            }
            if (ProjectResourcesNeeded < ProjectMembers.Count)
            {
                ErrorMessage = "Resources needed cannot be less than the number of currently employed members.";
                return false;
            }
            if (EmployedSkillProviders.Count == 0 && (ProjectStatusSelected?.Equals(ProjectStatus.Completed) ?? false))
            {
                ErrorMessage = "A complete project cannot have no employed skill providers. If you want to close the project, mark it as deactivated instead.";
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

            if ((ProjectStatusSelected == ProjectStatus.Deactivated) && EmployedSkillProviders.Count > 0)
            {
                ErrorMessage = "Cannot deactivate project while there are still employed skill providers.";
                return false;
            }

            return true;
        }

        [RelayCommand]
        private async Task Submit()
        {
            if (!ValidateForm())
                return;

            if (ProjectStatusSelected == ProjectStatus.Deactivated && EmployedSkillProviders.Count > 0)
            {
                await Shell.Current.DisplayAlert("Error", "Cannot deactivate project while there are still employed skill providers.", "OK");
                return;
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(_sessionService.VisitingProjectID);
            if (project == null) return;

            // Confirm completion if needed
            if (ProjectStatusSelected == ProjectStatus.Completed)
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirm Completion",
                    "Are you sure you want to mark this project as completed? The project will be closed and cannot be updated further. You will also proceed to rate your employed skill providers.",
                    "Yes",
                    "No"
                );

                if (!confirm)
                    return;
            }

            // Handle resignations
            foreach (var skillProvider in _pendingRemovals)
            {
                // Remove from project members
                project.ProjectMembers.RemoveAll(m => m.MemberId == skillProvider.UserId);

                // Remove project from skill provider's employed list
                if (skillProvider.EmployedProjects.Contains(project.ProjectId))
                {
                    skillProvider.EmployedProjects.Remove(project.ProjectId);
                    await _unitOfWork.SkillProvider.UpdateAsync(skillProvider);
                }
            }

            // Handle rejected resignations: reset their flags
            foreach (var member in project.ProjectMembers)
            {
                if (member.IsResigning)
                {
                    member.IsResigning = false;
                    member.ResignationReason = null;
                }
            }

            // Update project details
            project.ProjectDescription = ProjectDescription;
            project.ProjectPriority = ProjectPrioritySelected;
            project.ProjectStartDate = ProjectStartDate;
            project.ProjectSkillsRequired = ProjectSkillsRequired.ToList();
            project.ProjectResourcesNeeded = ProjectResourcesNeeded;
            project.ProjectResourcesAvailable = project.ProjectResourcesNeeded - project.ProjectMembers.Count;
            project.ProjectStatus = ProjectStatusSelected;
            project.ProjectDateUpdated = DateTime.Now;

            if (project.ProjectStatus == ProjectStatus.Completed)
            {
                project.ProjectEndDate = ProjectEndDate;
                await _unitOfWork.Projects.UpdateAsync(project);
                await _unitOfWork.SaveChangesAsync();
                ErrorMessage = string.Empty;

                await ShowProjectSummary(project);
                await _navigationService.NavigateToAsync("/ProjectOwnerRateSkillProviderView");
            }
            else
            {
                await _unitOfWork.Projects.UpdateAsync(project);
                await _unitOfWork.SaveChangesAsync();
                ErrorMessage = string.Empty;

                await Shell.Current.DisplayAlert("Success", "Project updated successfully!", "OK");
                await _navigationService.GoBackAsync();
            }

            _pendingRemovals.Clear();
        }


        private async Task ShowProjectSummary(Project project)
        {
            // Calculate project duration
            var duration = project.ProjectEndDate - project.ProjectStartDate;
            var durationText = $"{(int)duration.TotalDays} days";

            // Build skill providers list
            var providersList = string.Join("\n", EmployedSkillProviders
                .Select(sp => $"- {sp.UserFirstName} {sp.UserLastName}"));

            // Build skills required list
            var skillsList = string.Join("\n", project.ProjectSkillsRequired
                .Select(s => $"- {s}"));

            await Shell.Current.DisplayAlert(
                "Actual Project Completion Details",
                $"Project: {project.ProjectName}\n\n" +
                $"Description: {project.ProjectDescription}\n\n" +
                $"Location: {project.ProjectLocation}\n" +
                $"Priority: {project.ProjectPriority}\n" +
                $"Status: {project.ProjectStatus}\n\n" +
                $"Duration: {durationText}\n" +
                $"Start: {project.ProjectStartDate:MMMM d, yyyy}\n" +
                $"End: {project.ProjectEndDate:MMMM d, yyyy}\n\n" +
                $"Resources Needed: {project.ProjectResourcesNeeded}\n" +
                $"Members Employed: {EmployedSkillProviders.Count}\n\n" +
                $"Skill Providers:\n{providersList}\n\n" +
                $"Skills Required:\n{skillsList}",
                "Continue to Rating");
        }

        [RelayCommand]
        private async Task Reset()
        {
            // Restore original state
            ProjectMembers = new ObservableCollection<ProjectMemberDetail>(_originalProjectMembers);
            EmployedSkillProviders = new ObservableCollection<SkillProvider>(_originalSkillProviders);
            _pendingRemovals.Clear();

            ProcessResignationRequest();

            await LoadCurrentProject(); // Reload other fields
        }

        [RelayCommand]
        private async Task Cancel()
        {
            var result = await Shell.Current.DisplayAlert("Cancel", "Are you sure you want to cancel?", "Yes", "No");
            if (result)
            {
                await _navigationService.GoBackAsync();
            }
        }

        [RelayCommand]
        private void AddSkill()
        {
            if (!string.IsNullOrWhiteSpace(SelectedSkill) && !ProjectSkillsRequired.Contains(SelectedSkill.Trim()))
            {
                ProjectSkillsRequired.Add(SelectedSkill.Trim());
                SelectedSkill = string.Empty;
            }
        }

        [RelayCommand]
        private void RemoveSkill(string skill)
        {
            if (ProjectSkillsRequired.Contains(skill))
                ProjectSkillsRequired.Remove(skill);
        }

        [RelayCommand]
        private async Task RemoveSkillProvider(SkillProvider skillProvider)
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Remove Skill Provider",
                "Are you sure you want to remove this skill provider? This change won't be permanent until you submit the form.",
                "Yes",
                "No");

            if (!confirm)
                return;

            // Mark for removal (won't actually remove from database until Submit)
            _pendingRemovals.Add(skillProvider);

            // Update UI collections
            EmployedSkillProviders.Remove(skillProvider);
            ProjectMembers.Remove(ProjectMembers.FirstOrDefault(m => m.MemberId == skillProvider.UserId));
        }

        private async Task ProcessResignationRequest()
        {
            if (ProjectMembers == null || ProjectMembers.Count == 0)
                return;

            _pendingRejections.Clear();

            foreach (var member in ProjectMembers.Where(m => m.IsResigning).ToList())
            {
                var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(member.MemberId);
                if (skillProvider == null) continue;

                string reasonText = string.IsNullOrWhiteSpace(member.ResignationReason)
                    ? "No reason was provided."
                    : member.ResignationReason;

                bool confirm = await Shell.Current.DisplayAlert(
                    "Resignation Request",
                    $"{skillProvider.UserFirstName} {skillProvider.UserLastName} has requested to resign.\n\nReason: {reasonText}\n\nDo you approve this resignation?\nThe change will be finalized once the project is updated.",
                    "Yes",
                    "No");

                if (confirm)
                {
                    _pendingRemovals.Add(skillProvider);
                }
                else
                {
                    _pendingRejections.Add(member);
                }

                await Shell.Current.DisplayAlert("Notice", "Please click Update Project to finalize your decision.", "OK");
            }
        }

    }
}