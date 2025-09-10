using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerBrowseSkillProviderView : ContentPage
{
	public ProjectOwnerBrowseSkillProviderView(BrowseSkillProviderViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BrowseSkillProviderViewModel vm)
            await vm.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}