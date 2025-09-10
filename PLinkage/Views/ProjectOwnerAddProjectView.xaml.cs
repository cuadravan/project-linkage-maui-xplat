using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerAddProjectView : ContentPage
{
	public ProjectOwnerAddProjectView(AddProjectViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}