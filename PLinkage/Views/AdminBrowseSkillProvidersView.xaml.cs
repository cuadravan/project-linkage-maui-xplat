using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class AdminBrowseSkillProvidersView : ContentPage
{
	public AdminBrowseSkillProvidersView(AdminBrowseSkillProviderViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminBrowseSkillProviderViewModel vm)
            await vm.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}