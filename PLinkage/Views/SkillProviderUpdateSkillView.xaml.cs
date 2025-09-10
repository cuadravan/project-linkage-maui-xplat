using PLinkage.ViewModels;
namespace PLinkage.Views;

public partial class SkillProviderUpdateSkillView : ContentPage
{
	public SkillProviderUpdateSkillView(UpdateSkillViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}