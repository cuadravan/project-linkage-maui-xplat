using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerOffersAndApplicationsView : ContentPage
{
	public ProjectOwnerOffersAndApplicationsView(ProjectOwnerApplicationOfferViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProjectOwnerApplicationOfferViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);
    }
}