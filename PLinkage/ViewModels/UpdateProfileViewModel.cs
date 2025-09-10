using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class UpdateProfileViewModel : ObservableValidator
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly INavigationService _navigationService;
        private readonly ISessionService _sessionService;

        private readonly UserRole? currentUserRole;
        private ProjectOwner? projectOwner;
        private SkillProvider? skillProvider;



        // Fields
        [ObservableProperty, Required(ErrorMessage = "First Name is required"),
         RegularExpression(@"^[A-Z][a-zA-Z0-9]*(\s[A-Z][a-zA-Z0-9]*)*$", ErrorMessage = "Please enter a valid First Name.")]
        private string firstName;

        [ObservableProperty, Required(ErrorMessage = "Last Name is required"),
         RegularExpression(@"^[A-Z][a-zA-Z0-9]*(\s[A-Z][a-zA-Z0-9]*)*$", ErrorMessage = "Please enter a valid Last Name.")]
        private string lastName;

        [ObservableProperty] private DateTime birthdate = DateTime.Now;
        [ObservableProperty] private bool isMale;
        [ObservableProperty] private bool isFemale;

        [ObservableProperty,
         Required(ErrorMessage = "Mobile number is required."),
         RegularExpression(@"^\d{10,11}$", ErrorMessage = "Mobile number must be 10–11 digits.")]
        private string mobileNumber;


        [ObservableProperty] private CebuLocation? selectedLocation;
        [ObservableProperty] private string errorMessage;

        public ObservableCollection<CebuLocation> CebuLocations { get; } =
            new(Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        // Constructor
        public UpdateProfileViewModel(IUnitOfWork unitOfWork, INavigationService navigationService, ISessionService sessionService)
        {
            _unitOfWork = unitOfWork;
            _navigationService = navigationService;
            _sessionService = sessionService;
            currentUserRole = _sessionService.GetCurrentUserType(); // returns UserRole
            _ = LoadCurrentProfile();
        }

        // Core Methods
        private async Task LoadCurrentProfile()
        {
            var userId = _sessionService.GetCurrentUser().UserId;

            switch (currentUserRole)
            {
                case UserRole.ProjectOwner:
                    projectOwner = await _unitOfWork.ProjectOwner.GetByIdAsync(userId);
                    if (projectOwner == null) return;

                    FirstName = projectOwner.UserFirstName;
                    LastName = projectOwner.UserLastName;
                    Birthdate = projectOwner.UserBirthDate;
                    MobileNumber = projectOwner.UserPhone;
                    SelectedLocation = projectOwner.UserLocation;
                    IsMale = projectOwner.UserGender == "Male";
                    IsFemale = projectOwner.UserGender == "Female";
                    break;

                case UserRole.SkillProvider:
                    skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(userId);
                    if (skillProvider == null) return;

                    FirstName = skillProvider.UserFirstName;
                    LastName = skillProvider.UserLastName;
                    Birthdate = skillProvider.UserBirthDate;
                    MobileNumber = skillProvider.UserPhone;
                    SelectedLocation = skillProvider.UserLocation;
                    IsMale = skillProvider.UserGender == "Male";
                    IsFemale = skillProvider.UserGender == "Female";
                    break;
            }
        }

        private bool IsAtLeast18YearsOld(DateTime birthdate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthdate.Year;
            if (birthdate > today.AddYears(-age)) age--;
            return age >= 18;
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

            if (!IsAtLeast18YearsOld(Birthdate))
                return SetError("You must be at least 18 years old.");

            if (!(IsMale || IsFemale))
                return SetError("Please select a gender.");

            if (!SelectedLocation.HasValue)
                return SetError("Please select a location.");

            return true;
        }


        private bool SetError(string message)
        {
            ErrorMessage = message;
            return false;
        }

        // Commands
        [RelayCommand]
        private async Task Update()
        {
            if (!ValidateForm()) return;

            switch (currentUserRole)
            {
                case UserRole.ProjectOwner:
                    if (projectOwner == null) return;

                    projectOwner.UserFirstName = FirstName;
                    projectOwner.UserLastName = LastName;
                    projectOwner.UserBirthDate = Birthdate;
                    projectOwner.UserGender = IsMale ? "Male" : "Female";
                    projectOwner.UserPhone = MobileNumber;
                    projectOwner.UserLocation = SelectedLocation;

                    await _unitOfWork.ProjectOwner.UpdateAsync(projectOwner);
                    await _unitOfWork.SaveChangesAsync();
                    _sessionService.SetCurrentUser(projectOwner);
                    break;

                case UserRole.SkillProvider:
                    if (skillProvider == null) return;

                    skillProvider.UserFirstName = FirstName;
                    skillProvider.UserLastName = LastName;
                    skillProvider.UserBirthDate = Birthdate;
                    skillProvider.UserGender = IsMale ? "Male" : "Female";
                    skillProvider.UserPhone = MobileNumber;
                    skillProvider.UserLocation = SelectedLocation;

                    await _unitOfWork.SkillProvider.UpdateAsync(skillProvider);
                    await _unitOfWork.SaveChangesAsync();
                    _sessionService.SetCurrentUser(skillProvider);
                    break;
            }

            ErrorMessage = string.Empty;
            await _navigationService.GoBackAsync();
        }


        [RelayCommand]
        private Task Clear() => LoadCurrentProfile();

        [RelayCommand]
        private async Task BackToProfile()
        {
            _sessionService.VisitingProjectID = Guid.Empty;
            await _navigationService.GoBackAsync();
        }
    }
}
