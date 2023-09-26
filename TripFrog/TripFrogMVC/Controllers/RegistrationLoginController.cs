using Microsoft.AspNetCore.Mvc;
using TripFrogMVC.Services;
using TripFrogMVC.Services.WebApiClients;
using TripFrogWebApi.DTO;

namespace TripFrogMVC.Controllers;

public class RegistrationLoginController : Controller
{
    private readonly MemoryCacheManagerService _cacheManager;
    private readonly HttpClientFactory _httpClientFactory;

    public RegistrationLoginController(WebApiInfoService webApiInfo, MemoryCacheManagerService cacheManager)
    {
        _cacheManager = cacheManager;
        _httpClientFactory = new HttpClientFactory(webApiInfo, _cacheManager);
    }

    public IActionResult RawRegister()
    {
        return View("Register");
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterUserDto newUser)
    {
        using var client = _httpClientFactory.GetHttpClient();

        var response = await client.PostAsJsonAsync("api/Users/register", newUser);
        var deserializedResponse = await response.DeserializeResponseAsync<Response<UserDto>>();

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, deserializedResponse.Message);
            return View(newUser);
        }
        
        return RedirectToAction("RawLogin", "RegistrationLogin");
    }

    public IActionResult RawLogin()
    {
        return View("Login");
    }
    
    public async Task<IActionResult> Login(LoginUserCredentialsDto loginUserCredentials)
    {
        using var client = _httpClientFactory.GetHttpClient();

        var response = await client.PostAsJsonAsync("api/Users/login", loginUserCredentials);
        var deserializedResponse = await response.DeserializeResponseAsync<Response<LoginInfoDto>>();

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, deserializedResponse.Message);
            return View(loginUserCredentials);
        }

        _cacheManager.SetJwtToken(deserializedResponse.Data.Tokens.JwtToken);
        _cacheManager.SetLoggedUser(deserializedResponse.Data.LoggedUser);

        return RedirectToAction("ShowUserInfo", "UserInfo");
    }
}
