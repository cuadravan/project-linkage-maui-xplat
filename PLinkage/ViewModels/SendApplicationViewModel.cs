using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLinkage.Models;
using PLinkage.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PLinkage.ViewModels
{
    public partial class SendApplicationViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        private Guid _projectId;
        private Guid _skillProviderId;

        // Properties
        [ObservableProperty] private string projectName;
        [ObservableProperty] private SkillProvider skillProviderApplying;
        [ObservableProperty] private string rateAsked;
        [ObservableProperty] private string timeFrameAsked;
        [ObservableProperty] private Project visitingProject;

        public string SkillProviderFullName =>
            SkillProviderApplying != null
                ? $"{SkillProviderApplying.UserFirstName} {SkillProviderApplying.UserLastName}"
                : string.Empty;

        partial void OnSkillProviderApplyingChanged(SkillProvider value)
        {
            OnPropertyChanged(nameof(SkillProviderFullName));
        }

        public SendApplicationViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;
            LoadDetailsCommand = new AsyncRelayCommand(LoadDetails);
        }

        public IAsyncRelayCommand LoadDetailsCommand { get; }

        private async Task LoadDetails()
        {
            await _unitOfWork.ReloadAsync();

            _projectId = _sessionService.VisitingProjectID;
            var currentUser = _sessionService.GetCurrentUser();

            if (currentUser != null && currentUser is SkillProvider provider)
            {
                _skillProviderId = provider.UserId;
                SkillProviderApplying = provider;
            }
            else
            {
                await Shell.Current.DisplayAlert("⚠️ Error", "Current user is not a valid Skill Provider.", "OK");
                await _navigationService.GoBackAsync();
                return;
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(_projectId);
            if (project == null)
            {
                await Shell.Current.DisplayAlert("⚠️ Error", "Project not found.", "OK");
                await _navigationService.GoBackAsync();
                return;
            }

            VisitingProject = project;
            ProjectName = project.ProjectName;
        }

        [RelayCommand]
        private async Task SendApplication()
        {
            if (VisitingProject == null ||
                string.IsNullOrWhiteSpace(RateAsked) ||
                string.IsNullOrWhiteSpace(TimeFrameAsked))
            {
                await Shell.Current.DisplayAlert("❗ Missing Info", "Please complete all fields before applying.", "OK");
                return;
            }

            if (!decimal.TryParse(RateAsked, out decimal rate))
            {
                await Shell.Current.DisplayAlert("❗ Invalid Rate", "Rate must be a valid number.", "OK");
                return;
            }

            if (!int.TryParse(TimeFrameAsked, out int hours))
            {
                await Shell.Current.DisplayAlert("❗ Invalid Timeframe", "Timeframe must be a number in hours.", "OK");
                return;
            }

            var allowedHours = (VisitingProject.ProjectEndDate - VisitingProject.ProjectStartDate).TotalHours;

            if (hours > allowedHours)
            {
                await Shell.Current.DisplayAlert("❗ Timeframe Too Long",
                    $"Your requested timeframe exceeds the project duration of {allowedHours:N0} hours.", "OK");
                return;
            }

            var application = new OfferApplication
            {
                OfferApplicationType = "Application",
                SenderId = _skillProviderId,
                ReceiverId = VisitingProject.ProjectOwnerId,
                ProjectId = VisitingProject.ProjectId,
                OfferApplicationStatus = "Pending",
                OfferApplicationRate = rate,
                OfferApplicationTimeFrame = hours
            };

            await _unitOfWork.OfferApplications.AddAsync(application);
            await _unitOfWork.SaveChangesAsync();

            // Link OfferApplication ID to both sender and receiver
            var projectOwner = await _unitOfWork.ProjectOwner.GetByIdAsync(VisitingProject.ProjectOwnerId);
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(_skillProviderId);

            if (projectOwner != null)
            {
                projectOwner.OfferApplicationId.Add(application.OfferApplicationId);
                await _unitOfWork.ProjectOwner.UpdateAsync(projectOwner);
            }

            if (skillProvider != null)
            {
                skillProvider.OfferApplicationId.Add(application.OfferApplicationId);
                await _unitOfWork.SkillProvider.UpdateAsync(skillProvider);
            }

            await _unitOfWork.SaveChangesAsync();

            await Shell.Current.DisplayAlert("✅ Application Sent", "You have successfully applied to the project.", "OK");
            await _navigationService.GoBackAsync();
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
