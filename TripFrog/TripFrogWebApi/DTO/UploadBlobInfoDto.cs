namespace TripFrogWebApi.DTO;

public class UploadBlobInfoDto :IUploadBlobInfoDto
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
}
