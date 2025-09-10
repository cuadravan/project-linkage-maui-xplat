using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class SkillProviderUpdateEducationView : ContentPage
{
	public SkillProviderUpdateEducationView(UpdateEducationViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}