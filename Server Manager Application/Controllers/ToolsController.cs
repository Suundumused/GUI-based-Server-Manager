using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Server_Manager_Application.Models.Options;
using Server_Manager_Application.Resources.Languages;
using Server_Manager_Application.Runtime.HighLevel;
using System.Text.Json;


namespace Server_Manager_Application.Controllers
{
    public class ToolsController : BaseController
    {
        public ToolsController(IOptions<BasicOptions> basicOptions) : base(basicOptions)
        {
        }

        [HttpGet]
        public IActionResult Console()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Console([FromBody] System.Text.Json.JsonElement jsonData)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { response = AppResources.ParserError, state = false });
            }

            if (jsonData.TryGetProperty("mString", out JsonElement commandElement))
            {
                string stringCommand = commandElement.ToString();

                switch (stringCommand.ToLower())
                {
                    case "freset":
                        await Program.commandRunner.CloseAsync();
                        Program.commandRunner = new CommandRunner();

                        return Json(new { response = AppResources.TerminalReboot, state = false });

                    case "fshutdown":
                        await Program.commandRunner.CloseAsync();
                        Environment.Exit(0);

                        break;
                }

                (string, string) cmdResult = await Program.commandRunner.ExecuteCommandAsync(stringCommand);

                return Json(new { response = cmdResult.Item1, state = cmdResult.Item2 });
            }

            return Json(new { response = AppResources.MissingField, state = false });
        }
        
        [HttpGet]
        public IActionResult Path()
        {
            return View();
        }
    }
}