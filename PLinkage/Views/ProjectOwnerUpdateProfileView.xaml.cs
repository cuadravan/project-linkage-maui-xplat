using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerUpdateProfileView : ContentPage
{
	public ProjectOwnerUpdateProfileView(UpdateProfileViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}