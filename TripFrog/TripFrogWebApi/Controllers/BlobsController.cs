using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.Services;

namespace TripFrogWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BlobsController : ControllerBase
{
    private IBlobStorageService _blobStorageService;

    public BlobsController(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }
    
    [HttpPost]
    public async Task<ActionResult<string>> UploadUsersAccountPhoto()
    {
        var file = HttpContext.Request.Form.Files[0];
        var photoLink = await _blobStorageService.UploadAccountPhotoGetCreatedUrl(file);
        return Ok(photoLink);
    }

    [HttpDelete("{photoUrl}")]
    public async Task<ActionResult> DeleteUsersAccountPhoto(string photoUrl)
    {
        var decodedPhotoUrl = HttpUtility.UrlDecode(photoUrl);
        await _blobStorageService.DeleteAccountPhotoByUrl(decodedPhotoUrl);
        return Ok();
    }
}
