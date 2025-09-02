using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

using System.Diagnostics;

using Server_Manager_Application.Models;
using Server_Manager_Application.Models.Options;


namespace Server_Manager_Application.Controllers
{
    public abstract class BaseController : Controller 
    {
        protected readonly BasicOptions _basicOptions;


        public BaseController(IOptions<BasicOptions> basicOptions)
        {
            _basicOptions = basicOptions.Value;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            GlobalViewBags();
            base.OnActionExecuting(context);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error() // !!! Sensitive zone - only if the user is logged in will exception handler information be displayed. !!!
        {
            HttpContext context = HttpContext;
            System.Security.Principal.IIdentity? identity = context.User.Identity;

            int status_code = context.Response.StatusCode;

            if (identity is not null) 
            {
                if (identity.IsAuthenticated)
                {
                    IExceptionHandlerPathFeature? exceptionHandlerPathMessage = context.Features.Get<IExceptionHandlerPathFeature>();

                    if (exceptionHandlerPathMessage?.Error is not null)
                    {
                        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? context.TraceIdentifier, ErrorCode = status_code, ErrorDescription = ReasonPhrases.GetReasonPhrase(status_code), ErrorMessage = exceptionHandlerPathMessage.Error.Message });
                    }
                }
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? context.TraceIdentifier, ErrorCode = status_code, ErrorDescription = ReasonPhrases.GetReasonPhrase(status_code)});
        }

        private void GlobalViewBags() 
        {
            ViewBag.AppName = _basicOptions.AppName;
            ViewBag.CompanyName = _basicOptions.CompanyName;
        }

        public BasicOptions getBasicOptions() { return _basicOptions; }
    }
}
