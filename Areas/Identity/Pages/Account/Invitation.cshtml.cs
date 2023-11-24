using IsometricShooterWebApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IsometricShooterWebApp.Areas.Identity.Pages.Account
{
    public class InvitationModel : PageModel
    {
        private readonly UserManager<UserModel> _userManager;

        public InvitationModel(UserManager<UserModel> userManager)
        {
            this._userManager = userManager;
        }
        
        public async Task<IActionResult> OnGetAsync(string? token = null)
        {
            if (!Request.Cookies.ContainsKey("inv"))
            {

                var invUser = await _userManager.FindByIdAsync(token);

                if (invUser != null)
                {
                    Response.Cookies.Append("inv", token, new CookieOptions() { Expires = DateTime.UtcNow.AddDays(30) });

                    ++invUser.InvitationRedirectCount;

                    await _userManager.UpdateAsync(invUser);
                }

            }

            return LocalRedirect("/Identity/Account/Register");
        }
    }
}
