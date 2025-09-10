using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class RegisterViewModel : ObservableValidator
    {
        // Services
        private readonly INavigationService _navigationService;
        private readonly IUnitOfWork _unitOfWork;


        // Constructor
        public RegisterViewModel(INavigationService navigationService, IUnitOfWork unitOfWork)
        {
            _navigationService = navigationService;
            _unitOfWork = unitOfWork;
            ValidateAllProperties();
        }

        // Form fields
        [ObservableProperty, Required(ErrorMessage = "First Name is required"),
         RegularExpression(@"^[A-Z][a-zA-Z0-9]*(\s[A-Z][a-zA-Z0-9]*)*$", ErrorMessage = "Please enter a valid First Name.")]
        private string firstName;

        [ObservableProperty, Required(ErrorMessage = "Last Name is required"),
         RegularExpression(@"^[A-Z][a-zA-Z0-9]*(\s[A-Z][a-zA-Z0-9]*)*$", ErrorMessage = "Please enter a valid Last Name.")]
        private string lastName;

        [ObservableProperty, Required(ErrorMessage = "Email is required"),
         EmailAddress(ErrorMessage = "Please enter a valid Email Address.")]
        private string email;

        [ObservableProperty, Required(ErrorMessage = "Password is required"),
         MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        private string password;

        [ObservableProperty, Required(ErrorMessage = "Confirm Password is required")]
        private string confirmPassword;

        [ObservableProperty] private DateTime birthdate = DateTime.Now;
        [ObservableProperty] private bool isMale;
        [ObservableProperty] private bool isFemale;

        [ObservableProperty,
         Required(ErrorMessage = "Mobile Number is required."),
         RegularExpression(@"^\d{10,11}$", ErrorMessage = "Mobile number must be 10–11 digits.")]
        private string mobileNumber;


        [ObservableProperty] private CebuLocation? selectedLocation;
        [ObservableProperty, Required(ErrorMessage = "Please select a role.")] private string selectedRole;
        [ObservableProperty] private string errorMessage;

        public ObservableCollection<CebuLocation> CebuLocations { get; } =
            new(Enum.GetValues(typeof(CebuLocation)).Cast<CebuLocation>());

        public ObservableCollection<string> Roles { get; } = new()
        {
            "Skill Provider",
            "Project Owner"
        };

        private bool ValidateForm()
        {
            ErrorMessage = string.Empty;
            ValidateAllProperties();

            if (HasErrors)
            {
                ErrorMessage = GetErrors()
                    .OfType<ValidationResult>()
                    .FirstOrDefault()?.ErrorMessage;
                return false;
            }

            if (Password != ConfirmPassword)
                return SetError("Passwords do not match.");

            if (!(IsMale || IsFemale))
                return SetError("Please select a gender.");

            if (!SelectedLocation.HasValue)
                return SetError("Please select a location.");

            var today = DateTime.Today;
            var age = today.Year - Birthdate.Year;
            if (Birthdate.Date > today.AddYears(-age)) age--; // adjust for birthdate not yet occurred this year
            if (age < 18)
                return SetError("You must be 18 years old or above to register.");

            return true;
        }


        private bool SetError(string message)
        {
            ErrorMessage = message;
            return false;
        }

        [RelayCommand]
        private async Task Register()
        {
            if (!ValidateForm()) return;

            var skillProviders = await _unitOfWork.SkillProvider.GetAllAsync();
            var projectOwners = await _unitOfWork.ProjectOwner.GetAllAsync();

            bool emailExists = skillProviders.Any(sp =>
                                    (string?)sp.GetType().GetProperty("UserEmail")?.GetValue(sp) == Email)
                            || projectOwners.Any(po =>
                                    (string?)po.GetType().GetProperty("UserEmail")?.GetValue(po) == Email);

            if (emailExists)
            {
                ErrorMessage = "Email is already registered. Please use a different email.";
                return;
            }

            if (SelectedRole == "Skill Provider")
            {
                var skillProvider = new SkillProvider
                {
                    UserFirstName = FirstName,
                    UserLastName = LastName,
                    UserEmail = Email,
                    UserPassword = Password,
                    UserBirthDate = Birthdate.Date,
                    UserGender = IsMale ? "Male" : "Female",
                    UserStatus = "Active",
                    UserPhone = MobileNumber,
                    UserLocation = SelectedLocation,
                    JoinedOn = DateTime.Now,
                };
                await _unitOfWork.SkillProvider.AddAsync(skillProvider);
            }
            else if (SelectedRole == "Project Owner")
            {
                var projectOwner = new ProjectOwner
                {
                    UserFirstName = FirstName,
                    UserLastName = LastName,
                    UserEmail = Email,
                    UserPassword = Password,
                    UserBirthDate = Birthdate.Date,
                    UserGender = IsMale ? "Male" : "Female",
                    UserStatus = "Active",
                    UserPhone = MobileNumber,
                    UserLocation = SelectedLocation,
                    JoinedOn = DateTime.Now,
                };
                await _unitOfWork.ProjectOwner.AddAsync(projectOwner);
            }

            await _unitOfWork.SaveChangesAsync();
            await Shell.Current.DisplayAlert("✅ Success", "You are successfully registered! Please login now.", "OK");
            ErrorMessage = string.Empty;
            await _navigationService.NavigateToAsync("LoginView");
        }

        [RelayCommand]
        private Task Clear()
        {
            FirstName = LastName = Email = Password = ConfirmPassword = MobileNumber = SelectedRole = ErrorMessage = string.Empty;
            IsMale = IsFemale = false;
            SelectedLocation = null;
            Birthdate = DateTime.Now;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task BackToLogin() => await _navigationService.NavigateToAsync("LoginView");
    }
}
