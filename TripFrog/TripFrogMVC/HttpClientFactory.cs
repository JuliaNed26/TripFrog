using System.Net.Http.Headers;
using TripFrogMVC.Services;
using TripFrogMVC.Services.WebApiClients;
using TripFrogWebApi.DTO;

namespace TripFrogMVC;

public sealed class HttpClientFactory
{
    private readonly WebApiInfoService _webApiInfoService;
    private readonly MemoryCacheManagerService _cacheManager;

    public HttpClientFactory(WebApiInfoService webApiInfoService, MemoryCacheManagerService cacheManager)
    {
        _webApiInfoService = webApiInfoService;
        _cacheManager = cacheManager;
    }

    public HttpClient GetHttpClientWithAuthorization()
    {
        var httpClient = GetHttpClient();
        var token = _cacheManager.GetJwtToken();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return httpClient;
    }

    public HttpClient GetHttpClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = _webApiInfoService.HostUrl
        };
        return httpClient;
    }
}
