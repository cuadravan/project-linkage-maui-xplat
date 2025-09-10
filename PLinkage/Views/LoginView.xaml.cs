using PLinkage.ViewModels;

namespace PLinkage.Views
{
    public partial class LoginView : ContentPage
    {
        public LoginView(LoginViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }

}
