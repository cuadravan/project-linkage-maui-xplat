using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class AdminHomeView : ContentPage
{
	public AdminHomeView(AdminHomeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AdminHomeViewModel vm)
            await vm.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}