using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class SkillProviderOffersAndApplicationsView : ContentPage
{
	public SkillProviderOffersAndApplicationsView(SkillProviderApplicationOfferViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SkillProviderApplicationOfferViewModel viewModel)
        {
            viewModel.LoadDataCommand.Execute(null);
        }
    }
}