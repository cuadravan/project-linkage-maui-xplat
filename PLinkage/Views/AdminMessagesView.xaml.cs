using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class AdminMessagesView : ContentPage
{
	public AdminMessagesView(ViewMessagesViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewMessagesViewModel vm)
            await vm.LoadChatSummariesCommand.ExecuteAsync(null);
    }
}