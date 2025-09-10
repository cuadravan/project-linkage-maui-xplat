using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ProjectOwnerSendMessageView : ContentPage
{
	public ProjectOwnerSendMessageView(SendMessageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((SendMessageViewModel)BindingContext).LoadDetailsCommand.Execute(null);
    }
}