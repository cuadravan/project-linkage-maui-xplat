using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class SkillProviderBrowseProjectsView : ContentPage
{
	public SkillProviderBrowseProjectsView(BrowseProjectViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BrowseProjectViewModel vm)
            await vm.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}