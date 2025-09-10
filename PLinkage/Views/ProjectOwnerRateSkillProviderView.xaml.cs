using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerRateSkillProviderView : ContentPage
{
	public ProjectOwnerRateSkillProviderView(RateSkillProviderViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((RateSkillProviderViewModel)BindingContext).OnAppearingCommand.Execute(null);
    }
}