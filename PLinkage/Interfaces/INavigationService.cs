namespace PLinkage.Interfaces
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);
        Task GoBackAsync();
        Task NavigateToRootAsync();
    }

}
