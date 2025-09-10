using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class AdminBrowseProjectsView : ContentPage
{
	public AdminBrowseProjectsView(AdminBrowseProjectsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminBrowseProjectsViewModel vm)
            await vm.LoadProjectsCommand.ExecuteAsync(null);
    }
}