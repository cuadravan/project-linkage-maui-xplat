using Microsoft.Maui.Controls;
using PLinkage.Views;
using PLinkage.ViewModels;
using PLinkage.Interfaces;

namespace PLinkage
{
    public partial class AppShell : Shell
    {
        private readonly IStartupService _startupService;
        private bool _initialized;

        public AppShell(AppShellViewModel viewModel, IStartupService startupService)
        {
            InitializeComponent();

            Routing.RegisterRoute("ProjectOwnerUpdateProfileView", typeof(ProjectOwnerUpdateProfileView));
            Routing.RegisterRoute("ProjectOwnerAddProjectView", typeof(ProjectOwnerAddProjectView));
            Routing.RegisterRoute("ProjectOwnerUpdateProjectView", typeof(ProjectOwnerUpdateProjectView));
            Routing.RegisterRoute("ViewProjectView", typeof(ViewProjectView));
            Routing.RegisterRoute("ProjectOwnerRateSkillProviderView", typeof(ProjectOwnerRateSkillProviderView));
            Routing.RegisterRoute("ProjectOwnerSendOfferView", typeof(ProjectOwnerSendOfferView));
            Routing.RegisterRoute("ProjectOwnerSendMessageView", typeof(ProjectOwnerSendMessageView));
            Routing.RegisterRoute("ViewSkillProviderProfileView", typeof(ViewSkillProviderProfileView));
            Routing.RegisterRoute("SkillProviderAddEducationView", typeof(SkillProviderAddEducationView));
            Routing.RegisterRoute("SkillProviderUpdateEducationView", typeof(SkillProviderUpdateEducationView));
            Routing.RegisterRoute("SkillProviderAddSkillView", typeof(SkillProviderAddSkillView));
            Routing.RegisterRoute("SkillProviderUpdateSkillView", typeof(SkillProviderUpdateSkillView));
            Routing.RegisterRoute("SkillProviderSendApplicationView", typeof(SkillProviderSendApplicationView));
            Routing.RegisterRoute("ViewProjectOwnerProfileView", typeof(ViewProjectOwnerProfileView));
            Routing.RegisterRoute("RegisterView", typeof(RegisterView));
            _startupService = startupService;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_initialized)
                return;

            _initialized = true;
            await _startupService.StartAsync();
        }
    }
}
