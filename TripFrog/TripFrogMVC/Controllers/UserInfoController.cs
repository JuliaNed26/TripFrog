using Microsoft.AspNetCore.Mvc;
using TripFrogMVC.Services;
using TripFrogMVC.Services.WebApiClients;
using TripFrogWebApi.DTO;

namespace TripFrogMVC.Controllers
{
    public class UserInfoController : Controller
    {
        private readonly MemoryCacheManagerService _cacheManager;
        private readonly HttpClientFactory _httpClientFactory;
        private readonly BlobsApiClient _blobsClient;

        public UserInfoController(WebApiInfoService webApiInfo, MemoryCacheManagerService cacheManager, BlobsApiClient blobsClient)
        {
            _cacheManager = cacheManager;
            _httpClientFactory = new HttpClientFactory(webApiInfo, _cacheManager);
            _httpClientFactory.GetHttpClientWithAuthorization();
            _blobsClient = blobsClient;
        }

        [HttpGet]
        public IActionResult ShowUserInfo()
        {
            var user = _cacheManager.GetLoggedUser();

            return View(user as UserDto);
        }

        public IActionResult RawEdit()
        {
            var user = _cacheManager.GetLoggedUser();
            ViewData["FirstName"] = user.FirstName;
            ViewData["LastName"] = user.LastName;
            ViewData["Email"] = user.Email;
            ViewData["Phone"] = user.Phone;
            ViewData["AccountPhoto"] = user.PictureUrl;

            return View("EditUserInfo");
        }
        
        [HttpPost]
        public async Task<IActionResult> EditUserInfo(ChangedUserInfoDto changedUser)
        {
            using var client = _httpClientFactory.GetHttpClientWithAuthorization();
            var user = _cacheManager.GetLoggedUser();
            changedUser.Id = user.Id;

            await AddNewPhotoToChangeUserModelIfPhotoExists();
            
            var changeUserResponseJson = await client.PutAsJsonAsync("api/Users", changedUser);
            var changeUserResponse = await changeUserResponseJson.DeserializeResponseAsync<Response<UserDto>>();

            if (!changeUserResponseJson.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, changeUserResponse.Message);
                return View(changedUser);
            }

            _cacheManager.SetLoggedUser(changeUserResponse.Data);
            client.Dispose();

            return RedirectToAction("ShowUserInfo", "UserInfo");

            async Task AddNewPhotoToChangeUserModelIfPhotoExists()
            {
                var file = HttpContext.Request.Form.Files.GetFile("photoInput");
                if (file != null)
                {
                    if (!string.IsNullOrEmpty(user.PictureUrl))
                    {
                        await _blobsClient.DeleteFile(user.PictureUrl);
                    }
                    
                    changedUser.PictureUrl = await _blobsClient.UploadFileAndGetUrl(file);
                }
            }
        }
    }
}
