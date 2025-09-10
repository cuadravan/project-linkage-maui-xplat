using PLinkage.ViewModels;

namespace PLinkage.Views;

public partial class ViewProjectView : ContentPage
{
	public ViewProjectView(ViewProjectViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((ViewProjectViewModel)BindingContext).OnAppearingCommand.Execute(null);
    }
}