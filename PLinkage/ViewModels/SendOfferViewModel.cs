using CommunityToolkit.Mvvm.ComponentModel;
using PLinkage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PLinkage.Interfaces;
using PLinkage.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace PLinkage.ViewModels
{
    public partial class SendOfferViewModel: ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;
        private Guid _projectOwnerId = Guid.Empty;
        private Guid _skillProviderId = Guid.Empty;

        // Properties
        [ObservableProperty] private string projectName;
        // Project must be active
        [ObservableProperty] private SkillProvider skillProviderOffered;
        [ObservableProperty] private string rateOffered;
        // Time frame must be within time frame of project
        [ObservableProperty] private string timeFrameOffered;
        [ObservableProperty] private ObservableCollection<Project> ownedProjects = new();
        [ObservableProperty] private Project selectedProject;

        public string SkillProviderFullName =>
    SkillProviderOffered != null
        ? $"{SkillProviderOffered.UserFirstName} {SkillProviderOffered.UserLastName}"
        : string.Empty;

        partial void OnSkillProviderOfferedChanged(SkillProvider value)
        {
            OnPropertyChanged(nameof(SkillProviderFullName));
        }


        public SendOfferViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
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
            _skillProviderId = _sessionService.VisitingSkillProviderID;
            var currentUser = _sessionService.GetCurrentUser();
            if (currentUser != null)
            {
                _projectOwnerId = currentUser.UserId;
            }

            var projects = await _unitOfWork.Projects.GetAllAsync();
            var owned = projects.Where(p =>
                                       p.ProjectOwnerId == _projectOwnerId &&
                                       p.ProjectStatus == ProjectStatus.Active);
            if (!owned.Any())
            {
                await Shell.Current.DisplayAlert("⚠️ Warning", "You have no active projects, you cannot send offer.", "OK");
                await _navigationService.GoBackAsync();
            }
            OwnedProjects = new ObservableCollection<Project>(owned);

            // Add this block to fetch and set the skill provider
            var provider = await _unitOfWork.SkillProvider.GetByIdAsync(_skillProviderId);
            if (provider != null)
            {
                SkillProviderOffered = provider; // This triggers OnSkillProviderOfferedChanged
            }
        }


        [RelayCommand]
        private async Task SendOffer()
        {
            if (SelectedProject == null ||
                string.IsNullOrWhiteSpace(RateOffered) ||
                string.IsNullOrWhiteSpace(TimeFrameOffered))
            {
                await Shell.Current.DisplayAlert("❗ Missing Info", "Please complete all fields before sending the offer.", "OK");
                return;
            }

            // Validate rate
            if (!decimal.TryParse(RateOffered, out decimal rate))
            {
                await Shell.Current.DisplayAlert("❗ Invalid Rate", "Rate must be a valid number (e.g., 500 or 750.50).", "OK");
                return;
            }

            // Validate timeframe in hours
            if (!int.TryParse(TimeFrameOffered, out int hours))
            {
                await Shell.Current.DisplayAlert("❗ Invalid Timeframe", "Timeframe must be a number representing hours.", "OK");
                return;
            }

            // Validate project duration
            var projectStart = SelectedProject.ProjectStartDate;
            var projectEnd = SelectedProject.ProjectEndDate;

            if (projectEnd < projectStart)
            {
                await Shell.Current.DisplayAlert("❗ Project Date Error", "The selected project has an invalid date range.", "OK");
                return;
            }

            var allowedHours = (projectEnd - projectStart).TotalHours;
            if (hours > allowedHours)
            {
                await Shell.Current.DisplayAlert("❗ Timeframe Too Long",
                    $"The offered timeframe exceeds the project's duration of {allowedHours:N0} hours.", "OK");
                return;
            }

            // Prevent offering to someone already on this project
            if (SelectedProject.ProjectMembers.Any(m => m.MemberId == _skillProviderId))
            {
                await Shell.Current.DisplayAlert(
                    "❗ Already Employed",
                    "You cannot send an offer to a skill provider who is already employed on this project.",
                    "OK");
                return;
            }

            // CHECK: is the project full? ———
            var project = await _unitOfWork.Projects.GetByIdAsync(SelectedProject.ProjectId);
            if (project == null)
            {
                await Shell.Current.DisplayAlert("❗ Error", "Project not found.", "OK");
                return;
            }

            if (project.ProjectStatus is not ProjectStatus.Active)
            {
                await Shell.Current.DisplayAlert(
                    "⚠️ Inactive Project",
                    "You can only send offers for projects that are currently active.",
                    "OK");
                return;
            }

            if (project.ProjectResourcesAvailable <= 0)
            {
                await Shell.Current.DisplayAlert(
                    "⚠️ Project Full",
                    "This project has reached its maximum number of members and is no longer accepting offers.",
                    "OK");
                return; 
            }

            // Create and save OfferApplication
            var offer = new OfferApplication
            {
                OfferApplicationType = "Offer",
                SenderId = _projectOwnerId,
                ReceiverId = _skillProviderId,
                ProjectId = SelectedProject.ProjectId,
                OfferApplicationStatus = "Pending",
                OfferApplicationRate = rate,
                OfferApplicationTimeFrame = hours
            };

            await _unitOfWork.OfferApplications.AddAsync(offer);
            await _unitOfWork.SaveChangesAsync(); // Save to generate the OfferApplicationId

            // Update both ProjectOwner and SkillProvider with the new OfferApplicationId
            var projectOwner = await _unitOfWork.ProjectOwner.GetByIdAsync(_projectOwnerId);
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(_skillProviderId);

            if (projectOwner != null)
            {
                projectOwner.OfferApplicationId.Add(offer.OfferApplicationId);
                await _unitOfWork.ProjectOwner.UpdateAsync(projectOwner);
            }

            if (skillProvider != null)
            {
                skillProvider.OfferApplicationId.Add(offer.OfferApplicationId);
                await _unitOfWork.SkillProvider.UpdateAsync(skillProvider);
            }

            await _unitOfWork.SaveChangesAsync();

            await Shell.Current.DisplayAlert("✅ Success", "Offer successfully sent!", "OK");
            await _navigationService.GoBackAsync();
        }





        [RelayCommand]
        private async Task GoBack()
        {
            await _navigationService.GoBackAsync();
        }

    }
}
