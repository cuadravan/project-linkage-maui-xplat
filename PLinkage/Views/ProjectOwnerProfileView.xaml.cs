using PLinkage.ViewModels;
using CommunityToolkit.Mvvm.Input;


namespace PLinkage.Views;


public partial class ProjectOwnerProfileView : ContentPage
{
	public ProjectOwnerProfileView(ProjectOwnerProfileViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ProjectOwnerProfileViewModel vm)
        {
            vm.OnViewAppearingCommand.Execute(null);
        }
    }

}