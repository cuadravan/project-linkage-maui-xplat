using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class SkillProviderApplicationOfferViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        // Properties
        [ObservableProperty] private List<OfferApplicationDisplayModel> sentApplicationsPending;
        [ObservableProperty] private List<OfferApplicationDisplayModel> receivedOffersPending;
        [ObservableProperty] private List<OfferApplicationDisplayModel> sentApplicationsHistory;
        [ObservableProperty] private List<OfferApplicationDisplayModel> receivedOffersHistory;


        public SkillProviderApplicationOfferViewModel(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;

            LoadDataCommand = new AsyncRelayCommand(LoadData);
            ApproveOfferCommand = new AsyncRelayCommand<OfferApplicationDisplayModel>(ApproveOffer);
            RejectOfferCommand = new AsyncRelayCommand<OfferApplicationDisplayModel>(RejectOffer);
        }

        public IAsyncRelayCommand LoadDataCommand { get; }
        public IAsyncRelayCommand<OfferApplicationDisplayModel> ApproveOfferCommand { get; }
        public IAsyncRelayCommand<OfferApplicationDisplayModel> RejectOfferCommand { get; }


        private async Task LoadData()
        {
            await _unitOfWork.ReloadAsync();
            var currentUser = _sessionService.GetCurrentUser();
            if (currentUser == null) return;

            var skillProviderId = currentUser.UserId;
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(skillProviderId);
            if (skillProvider == null) return;

            var offerApplicationIds = skillProvider.OfferApplicationId;
            var allOffers = await _unitOfWork.OfferApplications.GetAllAsync();

            // Load supporting entities
            var allProjects = await _unitOfWork.Projects.GetAllAsync();
            var allSkillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            var allProjectOwners = await _unitOfWork.ProjectOwner.GetAllAsync();

            string GetProjectName(Guid id) => allProjects.FirstOrDefault(p => p.ProjectId == id)?.ProjectName ?? "(Unknown Project)";
            string GetUserName(Guid id)
            {
                var sp = allSkillProviders.FirstOrDefault(s => s.UserId == id);
                if (sp != null)
                    return $"{sp.UserFirstName} {sp.UserLastName}";

                var po = allProjectOwners.FirstOrDefault(p => p.UserId == id);
                if (po != null)
                    return $"{po.UserFirstName} {po.UserLastName}";

                return "(Unknown User)";
            }


            Func<OfferApplication, OfferApplicationDisplayModel> toDisplay = o => new OfferApplicationDisplayModel
            {
                OfferApplicationId = o.OfferApplicationId,
                ProjectName = GetProjectName(o.ProjectId),
                SenderName = GetUserName(o.SenderId),
                ReceiverName = GetUserName(o.ReceiverId),
                OfferApplicationType = o.OfferApplicationType,
                OfferApplicationStatus = o.OfferApplicationStatus,
                FormattedRate = $"₱{o.OfferApplicationRate:N2} per hour",
                FormattedTimeFrame = $"{o.OfferApplicationTimeFrame:N0} hrs"
            };

            SentApplicationsPending = allOffers
                .Where(o => offerApplicationIds.Contains(o.OfferApplicationId)
                            && o.SenderId == skillProviderId
                            && o.OfferApplicationType == "Application"
                            && o.OfferApplicationStatus == "Pending")
                .Select(toDisplay).ToList();

            ReceivedOffersPending = allOffers
                .Where(o => offerApplicationIds.Contains(o.OfferApplicationId)
                            && o.ReceiverId == skillProviderId
                            && o.OfferApplicationType == "Offer"
                            && o.OfferApplicationStatus == "Pending")
                .Select(toDisplay).ToList();

            SentApplicationsHistory = allOffers
                .Where(o => offerApplicationIds.Contains(o.OfferApplicationId)
                            && o.SenderId == skillProviderId
                            && o.OfferApplicationType == "Application"
                            && (o.OfferApplicationStatus == "Accepted" || o.OfferApplicationStatus == "Rejected"))
                .Select(toDisplay).ToList();

            ReceivedOffersHistory = allOffers
                .Where(o => offerApplicationIds.Contains(o.OfferApplicationId)
                            && o.ReceiverId == skillProviderId
                            && o.OfferApplicationType == "Offer"
                            && (o.OfferApplicationStatus == "Accepted" || o.OfferApplicationStatus == "Rejected"))
                .Select(toDisplay).ToList();
        }


        private async Task ApproveOffer(OfferApplicationDisplayModel display)
        {
            if (display == null) return;

            var application = await _unitOfWork.OfferApplications.GetByIdAsync(display.OfferApplicationId);
            if (application == null) return;

            // Load related project
            var project = await _unitOfWork.Projects.GetByIdAsync(application.ProjectId);
            if (project == null) return;

            // Check if project already has required number of members
            if (project.ProjectMembers.Count >= project.ProjectResourcesNeeded)
            {
                await Shell.Current.DisplayAlert("❗ Limit Reached",
                    $"This project already has the required number of members ({project.ProjectResourcesNeeded}).",
                    "OK");
                return;
            }

            if (project.ProjectStatus != ProjectStatus.Active)
            {
                await Shell.Current.DisplayAlert("❗ Project unavailable",
                    $"This project is not active anymore. It is {project.ProjectStatus}.",
                    "OK");
                return;
            }

            // Load skill provider (application sender)
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(application.ReceiverId);
            if (skillProvider == null) return;

            // Load the associated project
            if (project?.ProjectMembers != null &&
                project.ProjectMembers.Any(m => m.MemberId == skillProvider.UserId))
            {
                application.OfferApplicationStatus = "Rejected";
                await _unitOfWork.OfferApplications.UpdateAsync(application);
                await _unitOfWork.SaveChangesAsync();
                await LoadData();
                await Shell.Current.DisplayAlert("ℹ️ Application Rejected",
                    $"{skillProvider.UserFirstName} {skillProvider.UserLastName} is already a member of the project.",
                    "OK");
                return;
            }

            // Mark as accepted
            application.OfferApplicationStatus = "Accepted";
            await _unitOfWork.OfferApplications.UpdateAsync(application);

            // Add member
            var member = new ProjectMemberDetail
            {
                MemberId = skillProvider.UserId,
                UserFirstName = skillProvider.UserFirstName,
                UserLastName = skillProvider.UserLastName,
                Email = skillProvider.UserEmail,
                Rate = application.OfferApplicationRate,
                TimeFrame = application.OfferApplicationTimeFrame
            };

            project.ProjectMembers.Add(member);
            project.ProjectDateUpdated = DateTime.Now;
            project.ProjectResourcesAvailable = project.ProjectResourcesNeeded - project.ProjectMembers.Count;

            skillProvider.EmployedProjects.Add(project.ProjectId);

            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();
            await LoadData();
        }




        private async Task RejectOffer(OfferApplicationDisplayModel display)
        {
            var entity = await _unitOfWork.OfferApplications.GetByIdAsync(display.OfferApplicationId);
            if (entity == null) return;

            entity.OfferApplicationStatus = "Rejected";
            await _unitOfWork.OfferApplications.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            await LoadData();
        }

        [RelayCommand]
        private async Task ViewProject(OfferApplicationDisplayModel display)
        {
            if (display == null) return;
            var offerApplication = await _unitOfWork.OfferApplications.GetByIdAsync(display.OfferApplicationId);
            if (offerApplication == null) return;
            // Navigate to the project details page
            _sessionService.VisitingProjectID = offerApplication.ProjectId;
            await _navigationService.NavigateToAsync("/ViewProjectView");
        }

        [RelayCommand]
        private async Task ViewSender(OfferApplicationDisplayModel display)
        {
            if (display == null) return;

            var offerApplication = await _unitOfWork.OfferApplications.GetByIdAsync(display.OfferApplicationId);
            if (offerApplication == null) return;

            _sessionService.VisitingProjectOwnerID = offerApplication.SenderId;
            await _navigationService.NavigateToAsync("/ViewProjectOwnerProfileView");
        }

        [RelayCommand]
        private async Task ViewReceiver(OfferApplicationDisplayModel display)
        {
            if (display == null) return;

            var offerApplication = await _unitOfWork.OfferApplications.GetByIdAsync(display.OfferApplicationId);
            if (offerApplication == null) return;

            _sessionService.VisitingProjectOwnerID = offerApplication.ReceiverId;
            await _navigationService.NavigateToAsync("/ViewProjectOwnerProfileView");
        }



    }
}
