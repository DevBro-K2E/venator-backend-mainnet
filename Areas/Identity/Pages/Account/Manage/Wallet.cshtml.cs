using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Data.RequestModels;
using IsometricShooterWebApp.Managers.Abstraction;
using IsometricShooterWebApp.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IsometricShooterWebApp.Areas.Identity.Pages.Account.Manage
{
    public class WalletModel : PageModel
    {
        private readonly UserManager<UserModel> userManager;
        private readonly IBlockchainManager blockchainManager;
        private readonly ApplicationDbContext dbContext;

        public WalletModel(
            UserManager<UserModel> userManager,
            IBlockchainManager blockchainManager,
            ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.blockchainManager = blockchainManager;
            this.dbContext = dbContext;
        }

        [Display(Name = "Balance")]
        public double CurrentBalance
            => User.Balance;

        [Display(Name = "Game coins")]
        public double CurrentMoney
            => User.Money;

        public string UserIdentity
            => User.Id;

        public UserModel User { get; private set; }

        public string Address
            => blockchainManager.GetAddress();

        public string BlockchainApiUrl
            => blockchainManager.GetBlockchainApiUrl();

        [BindProperty]
        public WithdrawalRequestModel WithdrawalInput { get; set; }


        private async Task LoadUser()
        {
            User = await userManager.GetUserAsync(base.User);
        }

        public async Task OnGetAsync()
        {
            await LoadUser();
        }

        public bool SuccessWithdrawal { get; private set; } = false;

        public string? WithdrawalTransactionId { get; private set; }

        public async Task OnPostWithdrawalAsync()
        {
            var query = WithdrawalInput;

            if (!ModelState.IsValid)
                return;

            bool success = await dbContext.TryUpdateAsync(async () =>
            {
                await LoadUser();

                if (query.Count > User.Balance || query.Count < 0.00000000000001)
                {
                    ModelState.AddModelError($"{nameof(WithdrawalRequestModel)}.{nameof(WithdrawalRequestModel.Count)}", $"Value must be in range 1 and you balance");

                    return false;
                }

                User.Balance -= query.Count;

                await userManager.UpdateAsync(User);

                return true;
            });

            if (!ModelState.IsValid)
                return;

            if (!success)
            {
                ModelState.AddModelError($"{nameof(WithdrawalRequestModel)}.{nameof(WithdrawalRequestModel.Address)}", "Service unavailable at now. Please try again later");
                return;
            }

            WithdrawalTransactionId = await blockchainManager.SendTransactionAsync(query.Address, Convert.ToDecimal(query.Count));

            if (WithdrawalTransactionId == null)
            {
                await dbContext.TryUpdateAsync(async () =>
                {
                    await LoadUser();

                    User.Balance += query.Count;

                    await userManager.UpdateAsync(User);

                    return true;
                }, byte.MaxValue);

                ModelState.AddModelError($"{nameof(WithdrawalRequestModel)}.{nameof(WithdrawalRequestModel.Address)}", $"Cannot send {query.Count} to address {query.Address}. Blockchain return error. Please contact administration");

                return;
            }

            SuccessWithdrawal = true;

            return;
        }
    }
}
