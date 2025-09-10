using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class SkillProviderMessagesView : ContentPage
{
	public SkillProviderMessagesView(ViewMessagesViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewMessagesViewModel viewModel)
        {
            viewModel.LoadChatSummariesCommand.Execute(null);
        }
    }
}