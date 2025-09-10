using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class RateSkillProviderViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;
        private Guid _projectId;

        public RateSkillProviderViewModel(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;

            OnAppearingCommand = new AsyncRelayCommand(OnAppearing);
        }

        [ObservableProperty] private string projectName;
        [ObservableProperty]
        private ObservableCollection<SkillProvider> skillProvidersToRate = new();

        private List<Guid> _projectMembersIdList = new();

        public IAsyncRelayCommand OnAppearingCommand { get; }

        // Core Methods
        public async Task OnAppearing()
        {
            try
            {
                _projectId = _sessionService.VisitingProjectID;
                if (_projectId == Guid.Empty) return;

                await _unitOfWork.ReloadAsync();
                await LoadCurrentProject();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnAppearing crashed: {ex.Message}");
                // Optional: Show alert if needed
            }
        }

        private async Task LoadCurrentProject()
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(_sessionService.VisitingProjectID);
            if (project == null) return;

            ProjectName = project.ProjectName;
            _projectMembersIdList = project.ProjectMembers.Select(pm => pm.MemberId).ToList();
            await LoadEmployedSkillProviders();
        }

        private async Task LoadEmployedSkillProviders()
        {
            var allSkillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            var filtered = allSkillProviders.Where(sp => _projectMembersIdList.Contains(sp.UserId));
            SkillProvidersToRate.Clear();
            foreach (var sp in filtered)
            {
                SkillProvidersToRate.Add(sp);
            }

        }

        [RelayCommand]
        public async Task SubmitRatings()
        {
            // First, validate that all providers have been rated
            if (SkillProvidersToRate.Any(sp => sp.TempRating <= 0))
            {
                await Shell.Current.DisplayAlert("Incomplete Ratings", "Please rate all skill providers before submitting.", "OK");
                return;
            }

            // Proceed if all have valid ratings
            var skillProviders = await _unitOfWork.SkillProvider.GetAllAsync();

            foreach (var skillProviderToRate in SkillProvidersToRate)
            {
                var provider = skillProviders.FirstOrDefault(sp => sp.UserId == skillProviderToRate.UserId);

                if (provider != null)
                {
                    provider.UserRatingTotal += skillProviderToRate.TempRating;
                    provider.UserRatingCount += 1;
                    provider.UserRating = provider.UserRatingTotal / provider.UserRatingCount;

                    await _unitOfWork.SkillProvider.UpdateAsync(provider);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _navigationService.NavigateToAsync("///ProjectOwnerProfileView");
        }



    }
}
