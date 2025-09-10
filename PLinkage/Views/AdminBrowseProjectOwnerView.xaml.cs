using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class AdminBrowseProjectOwnerView : ContentPage
{
	public AdminBrowseProjectOwnerView(AdminBrowseProjectOwnerViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminBrowseProjectOwnerViewModel vm)
            await vm.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}