using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class SkillProviderSendApplicationView : ContentPage
{
	public SkillProviderSendApplicationView(SendApplicationViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SendApplicationViewModel vm)
            await vm.LoadDetailsCommand.ExecuteAsync(null);
    }
}