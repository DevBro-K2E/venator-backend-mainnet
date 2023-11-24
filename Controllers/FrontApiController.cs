using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Managers;
using IsometricShooterWebApp.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IsometricShooterWebApp.Controllers
{
    [Route("frontapi")]
    [GameApiAuthorize]
    public class FrontApiController : ControllerBase
    {
        private readonly UserManager<UserModel> userManager;

        public FrontApiController(UserManager<UserModel> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountInfo()
        {
            var u = await userManager.GetUserAsync(User);

            return Ok(new { u.Email, u.Name });
        }
    }
}
