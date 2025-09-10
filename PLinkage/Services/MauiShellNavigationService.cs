using PLinkage.Interfaces;

namespace PLinkage.Services
{
    public class MauiShellNavigationService : INavigationService
    {
        private readonly ISessionService _sessionService;

        public MauiShellNavigationService(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        // The reason why we use 3 dash is because the route is instantiated in the shell and in separate
        // To cater to times when a back button is needed, we will need to hardcode it
        // Idea: Remember previous and current page to always know which page to go back to
        // Also remember: previousProject, previousSkillProvider
        public async Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            // If route starts with / or ///, use as-is.
            // Else, treat as shell route and prepend ///
            if (!route.StartsWith("/"))
            {
                route = "///" + route;
            }

            if (parameters == null)
                await Shell.Current.GoToAsync(route);
            else
                await Shell.Current.GoToAsync(route, parameters);
        }


        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task NavigateToRootAsync()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }   
    }

}
