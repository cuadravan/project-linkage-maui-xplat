using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ViewSkillProviderProfileView : ContentPage
{
	public ViewSkillProviderProfileView(ViewSkillProviderProfileViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewSkillProviderProfileViewModel vm)
            await vm.OnViewAppearingCommand.ExecuteAsync(null);
    }
}