using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Managers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using IsometricShooterWebApp.Data.RequestModels;
using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models.GameApi;
using IsometricShooterWebApp.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace IsometricShooterWebApp.Controllers.Front
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserModel> userManager;
        private readonly SignInManager<UserModel> signInManager;
        private readonly IUserStore<UserModel> userStore;
        private readonly IEmailSender emailSender;
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager apiUserManager;

        private IUserEmailStore<UserModel> emailStore => userStore as IUserEmailStore<UserModel>;

        public AuthController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IUserStore<UserModel> userStore, IEmailSender emailSender, ApplicationDbContext dbContext, UserManager apiUserManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userStore = userStore;
            this.emailSender = emailSender;
            this.dbContext = dbContext;
            this.apiUserManager = apiUserManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var user = new UserModel();

                await userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var userId = await userManager.GetUserIdAsync(user);

                    if (!string.IsNullOrWhiteSpace(Input.InviteToken))
                    {
                        var invUser = await userManager.FindByIdAsync(Input.InviteToken);

                        if (invUser != null)
                        {
                            ++invUser.InvitationRegistrationCount;

                            await userManager.UpdateAsync(invUser);
                        }
                    }

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    string? callbackUrl = default;

                    if (Input.ReturnUrl != default)
                    {
                        callbackUrl = UriUtils.ReplaceOrAppendParameters(
                            Input.ReturnUrl,
                            new Dictionary<string, string>() { 
                                { "code", code },
                                { "userId", userId }
                            });
                    }

                    callbackUrl ??= Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code },
                        protocol: Request.Scheme);

                    await emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    return Ok(new { success = new { message = "Account registered, an email has been sent to confirm your email address" } });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return this.FailWithMessage(ModelState.GetFirstError());
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var user = await signInManager.UserManager.FindByNameAsync(Input.LoginName);

                if (user == default)
                {
                    user = await signInManager.UserManager.FindByEmailAsync(Input.LoginName);

                    if (user == default)
                        return this.FailWithMessage("User not found");
                }

                var signResult = await signInManager.CheckPasswordSignInAsync(user, Input.Password, false);

                if (!signResult.Succeeded)
                {
                    if (signResult.IsLockedOut)
                        return this.FailWithMessage("User locked");

                    if (signResult.IsNotAllowed)
                        return this.FailWithMessage("User no confirmed");

                    return this.FailWithMessage("User not found");
                }
                await signInManager.SignInAsync(user, false, "ApiCookie");


                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                //Generate the user's cookie!
                var claimsIdentity = new ClaimsIdentity(claims, "ApiCookie");
                var authProperties = new AuthenticationProperties { IsPersistent = true };
                await HttpContext.SignInAsync("ApiCookie", new ClaimsPrincipal(claimsIdentity), authProperties);



                return Ok(new { success = new { message = "Successfully logged in"} });
            }

            // If we got this far, something failed, redisplay form
            return this.FailWithMessage(ModelState.GetFirstError());
        }

        [HttpPost("/api/token/refresh")]
        [Authorize(AuthenticationSchemes = "ApiCookie")]
        public async Task<IActionResult> RefreshToken()
        {
            var user = await signInManager.UserManager.GetUserAsync(User);

            await signInManager.SignInAsync(user, false);

            return this.SuccessWithMessage("Token Refreshed");
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return this.SuccessWithMessage("Successfully logged out user");
        }

        [HttpPost("send_confirm_email")]
        public async Task<IActionResult> SendConfirmEmail([FromBody] SendConfirmEmailRequestModel Input)
        {
            if (!ModelState.IsValid)
            {
                return this.FailWithMessage(ModelState.GetFirstError());
            }

            var user = await userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                return this.FailWithMessage("No email address found");
            }

            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string? callbackUrl = default;

            if (Input.ReturnUrl != default)
            {
                callbackUrl = UriUtils.ReplaceOrAppendParameters(
                    Input.ReturnUrl,
                    new Dictionary<string, string>() {
                                { "code", code },
                                { "userId", userId }
                    });
            }

            callbackUrl ??= Url.Action("ConfirmEmail", "Auth",
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            await emailSender.SendEmailAsync(
                Input.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");


            return Ok(new { success = new { message = "Email Sent" } });
        }

        [HttpPost("confirm_email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequestModel Input)
        {
            var user = await userManager.FindByIdAsync(Input.UserId);

            if (user == null)
                return this.FailWithMessage("No found");

            if (user.EmailConfirmed)
            {
                return this.FailWithMessage("That email address has already been confirmed");
            }

            Input.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));

            var result = await userManager.ConfirmEmailAsync(user, Input.Code);

            if (result.Succeeded)
                return Ok(new { success = new { message = "Email Confirmed" } });

            return this.FailWithMessage("Token expired");
        }

        [HttpPost("forgot_password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordQueryRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.FailWithMessage("Email not found or not confirmed");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                string? callbackUrl = default;

                if (Input.ReturnUrl != default)
                {
                    callbackUrl = UriUtils.ReplaceOrAppendParameters(
                        Input.ReturnUrl,
                        new Dictionary<string, string>() { { "code", code } });
                }

                callbackUrl ??= Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return Ok(new { success = new { message = "Reset password sended" } });
            }

            return this.FailWithMessage(ModelState.GetFirstError());
        }

        [HttpPost("password_reset")]
        public async Task<IActionResult> PasswordReset([FromBody] ResetPasswordRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return this.FailWithMessage("Email not found");
                }

                Input.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));

                var result = await userManager.ResetPasswordAsync(user, Input.Code, Input.NewPassword);

                if (result.Succeeded)
                {
                    return this.SuccessWithMessage("Success reset");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return this.FailWithMessage(ModelState.GetFirstError());
        }
    }
}
