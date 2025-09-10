using PLinkage.Interfaces;
using PLinkage.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLinkage.Services
{
    public class StartupService: IStartupService
    {
        private readonly SplashScreenPage _splashPage;
        private readonly INavigationService _navigationService;

        public StartupService(SplashScreenPage splashPage, INavigationService navigationService)
        {
            _splashPage = splashPage;
            _navigationService = navigationService;
        }

        public async Task StartAsync()
        {
            // Show splash screen as modal
            await Shell.Current.Navigation.PushModalAsync(_splashPage);

            // Run animation
            await _splashPage.RunAnimationAsync();

            // Dismiss splash screen
            await Shell.Current.Navigation.PopModalAsync();
        }
    }


}
