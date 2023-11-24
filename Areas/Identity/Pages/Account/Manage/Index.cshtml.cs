// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IsometricShooterWebApp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly ApplicationDbContext dbContext;

        public IndexModel(
            UserManager<UserModel> userManager,
            SignInManager<UserModel> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.dbContext = dbContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public string InvitationUrl { get; set; }

        public int GameCount { get; set; }

        public int WinCount { get; set; }

        public int DefeatCount => GameCount - WinCount;

        public int KillCount { get; set; }

        public int DeathCount { get; set; }

        public int InvitationRedirectCount { get; set; }

        public int InvitationRegistrationCount { get; set; }

        public DateTime LatestLogin { get; set; }

        public IEnumerable<UserSessionModel> UserSessions { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[BindProperty]
        //public InputModel Input { get; set; }

        ///// <summary>
        /////     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        /////     directly from your code. This API may change or be removed in future releases.
        ///// </summary>
        //public class InputModel
        //{
        //    /// <summary>
        //    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        //    ///     directly from your code. This API may change or be removed in future releases.
        //    /// </summary>
        //    //[Phone]
        //    //[Display(Name = "Phone number")]
        //    //public string PhoneNumber { get; set; }
        //}

        private async Task LoadAsync(UserModel user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            //Input = new InputModel
            //{
            //    PhoneNumber = phoneNumber
            //};

            GameCount = user.GameCount;

            WinCount = user.WinCount;

            KillCount = user.KillCount;

            DeathCount = user.DeathCount;

            UserSessions = dbContext.UserSessions
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.Date);

            LatestLogin = UserSessions
                .FirstOrDefault()?.Date ?? DateTime.UtcNow;

            InvitationRedirectCount = user.InvitationRedirectCount;

            InvitationRegistrationCount = user.InvitationRegistrationCount;

            InvitationUrl = Url.Page("/Account/Invitation", null, new { token = _userManager.GetUserId(User) }, null, Request.Host.Value);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        await LoadAsync(user);
        //        return Page();
        //    }

        //    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        //    if (Input.PhoneNumber != phoneNumber)
        //    {
        //        var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
        //        if (!setPhoneResult.Succeeded)
        //        {
        //            StatusMessage = "Unexpected error when trying to set phone number.";
        //            return RedirectToPage();
        //        }
        //    }

        //    await _signInManager.RefreshSignInAsync(user);
        //    StatusMessage = "Your profile has been updated";
        //    return RedirectToPage();
        //}
    }
}
