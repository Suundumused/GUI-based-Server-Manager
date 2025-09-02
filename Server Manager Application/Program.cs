using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;

using System.CommandLine;
using System.CommandLine.Parsing;

using Server_Manager_Application.Common.Logging.Console_Utils;
using Server_Manager_Application.Common.Security.Interface;
using Server_Manager_Application.Common.Security.Public;
using Server_Manager_Application.Models.Options;
using Server_Manager_Application.Resources.Languages;
using Server_Manager_Application.Runtime.HighLevel;


namespace Server_Manager_Application 
{
    public class Program 
    {
        public static CommandRunner commandRunner = new CommandRunner();

        public static bool development_state;

        private static Int16 maxLoginAttempts = 5;
        private static Int16 timeOut = 90;
        private static Int16 sessTimeOut = 360;


        public static void Main(string[] args) 
        {
            Option<Int16> loginAttemptsOption = new Option<Int16>("--login_attempts")
            {
                DefaultValueFactory = ParseResult => 5,
                Description = AppResources.LoginAttemptsOption
            };
            Option<Int16> timeOutOption = new Option<Int16>("--time_out")
            {
                DefaultValueFactory = ParseResult => 90,
                Description = AppResources.TimeOutOption
            };
            Option<Int16> sessionExpirationTime = new Option<Int16>("--sess-time_out")
            {
                DefaultValueFactory = ParseResult => 360,
                Description = AppResources.SessionExpirationTime
            };

            RootCommand? rootCommand = new RootCommand($"{AppResources.WelcomeTo} {AppResources.AppName}");

            rootCommand.Options.Add(loginAttemptsOption);
            rootCommand.Options.Add(timeOutOption);
            rootCommand.Options.Add(sessionExpirationTime);

            ParseResult? parseResult = rootCommand.Parse(args);

            try 
            {
                if (parseResult.GetValue(loginAttemptsOption) is Int16 typedLoginAttempts)
                {
                    maxLoginAttempts = typedLoginAttempts;
                }

                if (parseResult.GetValue(timeOutOption) is Int16 typedTimeOut)
                {
                    timeOut = typedTimeOut;
                }

                if (parseResult.GetValue(sessionExpirationTime) is Int16 typedSessTimeOut)
                {
                    sessTimeOut = typedSessTimeOut;
                }
            }
            catch (System.InvalidOperationException ex) 
            {
                Printer.Print(ex.Message);
                Environment.Exit(2);
            }

            IReadOnlyList<ParseError>? parseErros = parseResult.Errors;

            if (parseErros.Count > 0) 
            {
                foreach (ParseError parseError in parseErros)
                {
                    Printer.Print(parseError.Message, Printer.Type_Headers.ERROR);
                }

                Environment.Exit(2);
            }

            rootCommand = null;
            parseResult = null;

            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<BasicOptions>(builder.Configuration.GetSection("Basic"));
            builder.Services.AddControllersWithViews(options => 
                {
                    options.Filters.Add(new AuthorizeFilter());
                }
            );
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/User/Login";   // Redireciona se não estiver logado
                    options.LogoutPath = "/User/Logout";

                    options.Cookie.HttpOnly = true; // Protect from XSS
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only HTTPS
                    options.Cookie.SameSite = SameSiteMode.Strict; // Evade CSRF

                    options.ExpireTimeSpan = TimeSpan.FromSeconds(sessTimeOut); // Session expiration
                    options.SlidingExpiration = true; // Removes active session
                }
            );

            builder.Services.AddScoped<IService>(provider => new Service(timeOut, maxLoginAttempts));
            builder.Services.AddHttpContextAccessor();

            WebApplication? app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions // Security headers (behind reverse proxy support)
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            development_state = app.Environment.IsDevelopment();

            app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseExceptionHandler("/Home/Error");
            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            PasswordInput.SetSecurePassword();

            app.Run();
        }
    }
}