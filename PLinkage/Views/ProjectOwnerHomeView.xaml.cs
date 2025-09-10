using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerHomeView : ContentPage
{
	public ProjectOwnerHomeView(ProjectOwnerHomeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ProjectOwnerHomeViewModel vm)
            await vm.LoadDashboardDataCommand.ExecuteAsync(null);
    }

}