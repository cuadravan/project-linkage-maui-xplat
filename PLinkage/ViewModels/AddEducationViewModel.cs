using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class AddEducationViewModel : ObservableValidator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        public AddEducationViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;
            TimeGraduated = DateTime.Today;
        }

        // Fields
        [ObservableProperty]
        [Required(ErrorMessage = "Course Name is required.")]
        private string courseName;

        [ObservableProperty]
        [Required(ErrorMessage = "School is required.")]
        private string schoolAttended;

        [ObservableProperty]
        [Required(ErrorMessage = "Month and Year Graduated is required.")]
        private DateTime timeGraduated;

        [ObservableProperty]
        private string errorMessage;

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

            var userId = _sessionService.GetCurrentUser().UserId;
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(userId);
            if (skillProvider == null)
            {
                ErrorMessage = "SkillProvider not found.";
                return;
            }

            var education = new Education
            {
                CourseName = CourseName,
                SchoolAttended = SchoolAttended,
                TimeGraduated = TimeGraduated
            };

            skillProvider.Educations.Add(education);

            await _unitOfWork.SkillProvider.UpdateAsync(skillProvider);
            await _unitOfWork.SaveChangesAsync();

            _sessionService.SetCurrentUser(skillProvider);
            await Shell.Current.DisplayAlert("Success", "Education added successfully!", "OK");
            await _navigationService.GoBackAsync();
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
