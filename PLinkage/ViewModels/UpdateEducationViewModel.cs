using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class UpdateEducationViewModel : ObservableValidator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        private Education? targetEducation;

        public UpdateEducationViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;
            TimeGraduated = DateTime.Today;

            _ = LoadEducation();
        }

        // Fields
        [ObservableProperty]
        [Required(ErrorMessage = "Course Name is required.")]
        private string courseName;

        [ObservableProperty]
        [Required(ErrorMessage = "School is required.")]
        private string schoolAttended;

        [ObservableProperty]
        [Required(ErrorMessage = "Year Graduated is required.")]
        private DateTime timeGraduated;

        [ObservableProperty]
        private string errorMessage;

        // Load existing education
        private async Task LoadEducation()
        {
            var userId = _sessionService.GetCurrentUser().UserId;
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(userId);
            var index = _sessionService.VisitingSkillEducationID;

            if (skillProvider != null && index >= 0 && index < skillProvider.Educations.Count)
            {
                targetEducation = skillProvider.Educations[index];

                CourseName = targetEducation.CourseName;
                SchoolAttended = targetEducation.SchoolAttended;
                TimeGraduated = targetEducation.TimeGraduated;
            }
            else
            {
                ErrorMessage = "Unable to load education entry.";
            }
        }

        // Commands
        [RelayCommand]
        private async Task Save()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                ErrorMessage = GetErrors()
                    .OfType<ValidationResult>()
                    .FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
                return;
            }

            if (targetEducation == null)
            {
                ErrorMessage = "Education entry not found.";
                return;
            }

            targetEducation.CourseName = CourseName;
            targetEducation.SchoolAttended = SchoolAttended;
            targetEducation.TimeGraduated = TimeGraduated;

            var userId = _sessionService.GetCurrentUser().UserId;
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(userId);

            if (skillProvider != null)
            {
                await _unitOfWork.SkillProvider.UpdateAsync(skillProvider);
                await _unitOfWork.SaveChangesAsync();
                _sessionService.SetCurrentUser(skillProvider);
                await Shell.Current.DisplayAlert("Success", "Education updated successfully!", "OK");
                await _navigationService.GoBackAsync();
            }
            else
            {
                ErrorMessage = "SkillProvider not found.";
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
