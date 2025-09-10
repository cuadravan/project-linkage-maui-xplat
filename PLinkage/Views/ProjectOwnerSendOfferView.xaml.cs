using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerSendOfferView : ContentPage
{
	public ProjectOwnerSendOfferView(SendOfferViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SendOfferViewModel vm)
            await vm.LoadDetailsCommand.ExecuteAsync(null);
    }
}