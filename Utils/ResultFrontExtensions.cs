using Microsoft.AspNetCore.Mvc;

namespace IsometricShooterWebApp.Utils
{
    public static class ResultFrontExtensions
    {
        public static IActionResult SuccessWithMessage(this ControllerBase controller, string message)
            => controller.Ok(new { success = new { message = message } });

        public static IActionResult FailWithMessage(this ControllerBase controller, string message)
            => controller.BadRequest(new
            {
                fail = new
                {
                    message = message
                }
            });
    }
}
