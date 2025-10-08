using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.Net;
using System.Security.Claims;
using Server_Manager_Application.Models.Options;
using Server_Manager_Application.Models.Security;
using Server_Manager_Application.Resources.Languages;
using Server_Manager_Application.Services.Security.Interface;


namespace Server_Manager_Application.Controllers
{
    [AllowAnonymous]
    public class UserController : BaseController
    {
        private readonly IService _auth;

        
        public UserController(IService auth, IOptions<BasicOptions> basicOptions): base(basicOptions)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Login(string? optional_message = null)
        {
            if (optional_message != null) 
            {
                ViewBag.LoginState = optional_message;
                return View();
            }

            HttpContext context = HttpContext;
            System.Security.Principal.IIdentity? identity = context.User.Identity;

            if (identity is not null)
            {
                if (identity.IsAuthenticated)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Credentials _credentials)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LoginState = AppResources.FieldCantBeEmpty;

                return View();
            }

            IPAddress? remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress == null)
            {
                remoteIpAddress = IPAddress.Parse("0.0.0.0");
            }

            switch (await _auth.ValidateUserAsync(_credentials.Username, _credentials.Password, remoteIpAddress))
            {
                case 0:
                    ViewBag.LoginState = AppResources.MaxLoginAttempts;
                    break;

                case 1:
                    ViewBag.LoginState = AppResources.WrongPassword;
                    break;

                case 2:
                    List<Claim>? claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, _credentials.Username),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    ClaimsIdentity? claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string? optional_message = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User", new { optional_message = optional_message });
        }
    }
}
