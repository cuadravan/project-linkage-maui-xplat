using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerUpdateProjectView : ContentPage
{
	public ProjectOwnerUpdateProjectView(UpdateProjectViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((UpdateProjectViewModel)BindingContext).OnAppearingCommand.Execute(null);
    }
}