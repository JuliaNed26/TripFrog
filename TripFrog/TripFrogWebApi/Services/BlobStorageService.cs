using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace TripFrogWebApi.Services;

public sealed class BlobStorageService : IBlobStorageService
{
    private const string UsersAccountPhotoContainer = "tripfrogprofilephotos";

    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadAccountPhotoGetCreatedUrl(IFormFile photoToUpload)
    {
        var fileName = Guid.NewGuid() + photoToUpload.FileName;
        var blobClient = GetBlobClient(UsersAccountPhotoContainer, fileName);
        await using (var stream = photoToUpload.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, true);
        }
        return GetFileUrl(UsersAccountPhotoContainer, fileName);
    }

    public string GetFileUrl(string containerName, string fileName)
    {
        var blobClient = GetBlobClient(containerName, fileName);
        string sasToken = GetCreatedSasTokenForFile(containerName, fileName, blobClient);
        string blobUrlWithSasToken = blobClient.Uri + sasToken;
        return blobUrlWithSasToken;
    }

    public async Task DeleteAccountPhotoByUrl(string fileUrl)
    {
        var fileName = new Uri(fileUrl).Segments.Last();
        var blobClient = GetBlobClient(UsersAccountPhotoContainer, fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    private BlobClient GetBlobClient(string containerName, string fileName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(fileName);
        return blobClient;
    }

    private static string GetCreatedSasTokenForFile(string containerName, string fileName, BlobClient blobClient)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = fileName,
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(1)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        string sasToken = blobClient.GenerateSasUri(sasBuilder).Query;
        return sasToken;
    }
}
