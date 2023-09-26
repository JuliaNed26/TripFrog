using TripFrogWebApi.DTO;

namespace TripFrogWebApi.Services;

public interface IBlobStorageService
{
    Task<string> UploadAccountPhotoGetCreatedUrl(IFormFile photoToUpload);
    string GetFileUrl(string containerName, string fileName);
    Task DeleteAccountPhotoByUrl(string fileUrl);
}
