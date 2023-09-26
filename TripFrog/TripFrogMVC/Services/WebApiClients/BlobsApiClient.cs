using System.Web;

namespace TripFrogMVC.Services.WebApiClients;

public class BlobsApiClient : ApiClient
{

    public BlobsApiClient(WebApiInfoService webApiInfo, MemoryCacheManagerService cacheManager) : base(webApiInfo, cacheManager) {}

    public async Task DeleteFile(string fileUrl)
    {
        using var client = HttpClientFactory.GetHttpClientWithAuthorization();
        var encodedPhotoUrl = HttpUtility.UrlEncode(fileUrl);
        await client.DeleteAsync($"api/Blobs/{encodedPhotoUrl}");
    }

    public async Task<string> UploadFileAndGetUrl(IFormFile file)
    {
        using var client = HttpClientFactory.GetHttpClientWithAuthorization();
        var requestContentWithFile = new MultipartFormDataContent
        {
            { new StreamContent(file.OpenReadStream()), "userPhoto", file.FileName }
        };

        var uploadPhotoRequest = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress + "api/Blobs")
        { Content = requestContentWithFile };
        var uploadBlobResponse = await client.SendAsync(uploadPhotoRequest);

        return await uploadBlobResponse.Content.ReadAsStringAsync();
    }
}
