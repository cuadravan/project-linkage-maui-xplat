using PLinkage.ViewModels;
namespace PLinkage.Views;

public partial class SkillProviderAddSkillView : ContentPage
{
	public SkillProviderAddSkillView(AddSkillViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}