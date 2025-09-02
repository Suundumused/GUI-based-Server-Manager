using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

using System.Net;

using Server_Manager_Application.Common.Security.Interface;
using Server_Manager_Application.Models.Options;
using Server_Manager_Application.Resources.Languages;


namespace Server_Manager_Application.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, IOptions<BasicOptions> basicOptions) : base(basicOptions)
        {
            _logger = logger;
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            IPAddress? remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            
            IService? _auth = HttpContext.RequestServices.GetRequiredService<IService>();

            if (remoteIpAddress == null)
            {
                remoteIpAddress = IPAddress.Parse("0.0.0.0");
            }

            if (_auth is not null)
            {
                if (_auth.GetStaticRemoteAttempts().ContainsKey(remoteIpAddress)) 
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    context.Result = new RedirectToActionResult("Login", "User", new { optional_message = AppResources.AccessViolation });
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        public IActionResult Index()
        {
            UserLevelViewBags();
            return View();
        }

        [AllowAnonymous]
        public IActionResult? Privacy() //Vulnerability test for data leak on error screen
        {
            //return View();
            return null; //Force an error
        }

        private void UserLevelViewBags()
        {
            ViewBag.ProgramOption1 = AppResources.ProgramOption1;
            ViewBag.ProgramOption2 = AppResources.ProgramOption2;
        }
    }
}
