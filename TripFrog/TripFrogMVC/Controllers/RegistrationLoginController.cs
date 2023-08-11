using Microsoft.AspNetCore.Mvc;
using TripFrogWebApi.DTO;

namespace TripFrogMVC.Controllers
{
    public class RegistrationLoginController : Controller
    {
        private readonly string _baseUrl;

        public RegistrationLoginController(IConfiguration configuration) => _baseUrl = configuration.GetSection("WebApiUrls:Https").Value!;

        public IActionResult RawRegister()
        {
            return View("Register");
        }

        public async Task<IActionResult> Register(RegisterUserDto newUser)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                var response = await client.PostAsJsonAsync("api/Users/register", newUser);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty,
                        $"User was not registered. {response.Content.ReadAsStringAsync().Result}");
                    return View(newUser);
                }
            }
            
            return RedirectToAction("RawLogin", "RegistrationLogin");
        }

        public IActionResult RawLogin()
        {
            return View("Login");
        }

        public async Task<IActionResult> Login(LoginUserDto loginUser)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                var response = await client.PostAsJsonAsync("api/Users/login", loginUser);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"User was not found or error occured. {response.Content.ReadAsStringAsync().Result}");
                    return View(loginUser);
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
