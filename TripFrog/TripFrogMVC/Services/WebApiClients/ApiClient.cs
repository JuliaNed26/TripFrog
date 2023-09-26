namespace TripFrogMVC.Services.WebApiClients;

public abstract class ApiClient
{
    protected ApiClient(WebApiInfoService webApiInfo, MemoryCacheManagerService cacheManager)
    {
        HttpClientFactory = new HttpClientFactory(webApiInfo, cacheManager);
    }

    protected HttpClientFactory HttpClientFactory { get; init; }
}
