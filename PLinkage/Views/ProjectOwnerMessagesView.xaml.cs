using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerMessagesView : ContentPage
{
	public ProjectOwnerMessagesView(ViewMessagesViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((ViewMessagesViewModel)BindingContext).LoadChatSummariesCommand.Execute(null);
    }
}