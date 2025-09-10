using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PLinkage.Interfaces;
using PLinkage.Services;
using PLinkage.ViewModels;
using PLinkage.Repositories;
using PLinkage.Views;

namespace PLinkage;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddSingleton<ISessionService, SessionService>();
		builder.Services.AddTransient<INavigationService, MauiShellNavigationService>();
        builder.Services.AddTransient<IAuthenticationService, JsonAuthenticationService>();
		builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
		builder.Services.AddTransient<IStartupService, StartupService>();

        builder.Services.AddTransient<SplashScreenPage>();
        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<LoginView>();

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ProjectOwnerHomeViewModel>();
		builder.Services.AddTransient<ProjectOwnerProfileViewModel>();
		builder.Services.AddTransient<UpdateProfileViewModel>();
		builder.Services.AddTransient<AddProjectViewModel>();
		builder.Services.AddTransient<ViewProjectViewModel>();
		builder.Services.AddTransient<UpdateProjectViewModel>();
		builder.Services.AddTransient<RateSkillProviderViewModel>();
		builder.Services.AddTransient<BrowseSkillProviderViewModel>();
        builder.Services.AddTransient<ViewSkillProviderProfileViewModel>();
		builder.Services.AddTransient<SendOfferViewModel>();
        builder.Services.AddTransient<ProjectOwnerApplicationOfferViewModel>();
		builder.Services.AddTransient<SendMessageViewModel>();
        builder.Services.AddTransient<ViewMessagesViewModel>();
        builder.Services.AddTransient<SkillProviderHomeViewModel>();
		builder.Services.AddTransient<SkillProviderProfileViewModel>();
        builder.Services.AddTransient<AddEducationViewModel>();
        builder.Services.AddTransient<UpdateEducationViewModel>();
        builder.Services.AddTransient<AddSkillViewModel>();
        builder.Services.AddTransient<UpdateSkillViewModel>();
		builder.Services.AddTransient<BrowseProjectViewModel>();
        builder.Services.AddTransient<SendApplicationViewModel>();
        builder.Services.AddTransient<ViewProjectOwnerProfileViewModel>();
        builder.Services.AddTransient<SkillProviderApplicationOfferViewModel>();
        builder.Services.AddTransient<AdminHomeViewModel>();
        builder.Services.AddTransient<AdminBrowseProjectsViewModel>();
        builder.Services.AddTransient<AdminBrowseSkillProviderViewModel>();
        builder.Services.AddTransient<AdminBrowseProjectOwnerViewModel>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
