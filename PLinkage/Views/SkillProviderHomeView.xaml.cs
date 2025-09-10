using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class SkillProviderHomeView : ContentPage
{
	public SkillProviderHomeView(SkillProviderHomeViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is SkillProviderHomeViewModel vm)
            await vm.LoadDashboardDataCommand.ExecuteAsync(null);
    }
}