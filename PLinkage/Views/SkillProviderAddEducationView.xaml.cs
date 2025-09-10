using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class SkillProviderAddEducationView : ContentPage
{
	public SkillProviderAddEducationView(AddEducationViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}