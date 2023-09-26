namespace TripFrogMVC.Services;
public sealed class WebApiInfoService
{
    private readonly string _hostUrl;

    public WebApiInfoService(string hostUrl)
    {
        _hostUrl = hostUrl;
    }

    public Uri HostUrl => new Uri(_hostUrl);
}
