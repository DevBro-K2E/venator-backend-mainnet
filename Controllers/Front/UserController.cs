using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Data.RequestModels;
using IsometricShooterWebApp.Managers;
using IsometricShooterWebApp.Managers.Abstraction;
using IsometricShooterWebApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Zlib;
using System.Collections.Generic;
using static IsometricShooterWebApp.Areas.Identity.Pages.Account.Manage.WalletModel;

namespace IsometricShooterWebApp.Controllers.Front
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "ApiCookie")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<UserModel> userManager;
        private readonly ApplicationDbContext dbContext;
        private readonly IBlockchainManager blockchainManager;

        public UserController(UserManager<UserModel> userManager, ApplicationDbContext dbContext, IBlockchainManager blockchainManager)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.blockchainManager = blockchainManager;
        }

        [HttpPost("balance")]
        public async Task<IActionResult> Balance()
        {
            var u = await userManager.GetUserAsync(User);

            return Ok(new
            {
                BalanceCoins = u.SilverCoins,
                BalanceMoney = u.Money,
                RealBalance = u.Balance
            });
        }

        [HttpPost("profile")]
        public async Task<IActionResult> Profile()
        {
            var u = await userManager.GetUserAsync(User);
            /*Username
Email
Balance-coins
Balance-money
Total Game Stats : 
Games played
Wins
Defeats
Kills
Death
Last Game Stats : 
Total kills
Time alive
Round duration
Money earned
*/

            var lastGameStats = await dbContext.GameLogs
                .Where(x => x.UserId == u.Id)
                .OrderByDescending(x => x.CreateTime)
                .FirstOrDefaultAsync();

            int roundDurationSecs = 0;

            if (lastGameStats != null)
                roundDurationSecs = (await dbContext.Games
                    .FirstAsync(x => x.Id.Equals(lastGameStats.GameId)))
                    .RoundElapsedSecs;

            return Ok(new
            {
                User = new
                {
                    u.Id,
                    Username = u.Name,
                    u.Email,
                    BalanceCoins = u.SilverCoins,
                    BalanceMoney = u.Money,
                    RealBalance = u.Balance,
                    TotalGameStats = new
                    {
                        u.GameCount,
                        u.WinCount,
                        DefeatCount = u.GameCount - u.WinCount,
                        u.KillCount,
                        u.DeathCount
                    },
                    LastGameStats = lastGameStats == null ? null : new {
                        lastGameStats.Kills,
                        lastGameStats.Death,
                        lastGameStats.AliveSecs,
                        roundDurationSecs
                    }
                }
            });
        }


        [HttpPost("password_change")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequestModel query)
        {
            var user = await userManager.GetUserAsync(User);

            var result = await userManager.ChangePasswordAsync(user, query.CurrentPassword, query.NewPassword);

            if (!result.Succeeded)
                return this.FailWithMessage(result.Errors.First().Description);

            return this.SuccessWithMessage("Password Changed");
        }

        [HttpPost("get_wallet_addr")]
        public async Task<IActionResult> GetWalletAddress()
            => Ok(blockchainManager.GetAddress());

        [HttpPost("withdrawal_to_wallet")]
        public async Task<IActionResult> WithdrawalToWallet([FromBody] WithdrawalRequestModel query)
        {
            if (!ModelState.IsValid)
                return this.FailWithMessage(ModelState.GetFirstError());

            bool success = await dbContext.TryUpdateAsync(async () =>
            {
                var user = await userManager.GetUserAsync(User);

                if (query.Count > user.Balance || query.Count < 0.00000000000001)
                {
                    ModelState.AddModelError($"{nameof(WithdrawalRequestModel)}.{nameof(WithdrawalRequestModel.Count)}", $"Value must be in range 1 and you balance");

                    return false;
                }

                user.Balance -= query.Count;

                await userManager.UpdateAsync(user);

                return true;
            });

            if (!ModelState.IsValid)
                return this.FailWithMessage(ModelState.GetFirstError());

            if (!success)
            {
                ModelState.AddModelError($"{nameof(WithdrawalRequestModel)}.{nameof(WithdrawalRequestModel.Address)}", "Service unavailable at now. Please try again later");
                return this.FailWithMessage(ModelState.GetFirstError());
            }

            var WithdrawalTransactionId = await blockchainManager.SendTransactionAsync(query.Address, Convert.ToDecimal(query.Count));

            if (WithdrawalTransactionId == null)
            {
                await dbContext.TryUpdateAsync(async () =>
                {
                    var user = await userManager.GetUserAsync(User);

                    user.Balance += query.Count;

                    await userManager.UpdateAsync(user);

                    return true;
                }, byte.MaxValue);

                ModelState.AddModelError($"{nameof(WithdrawalRequestModel)}.{nameof(WithdrawalRequestModel.Address)}", $"Cannot send {query.Count} to address {query.Address}. Blockchain return error. Please contact administration");

                return this.FailWithMessage(ModelState.GetFirstError());
            }

            return this.SuccessWithMessage($"Success. TransactionId={WithdrawalTransactionId}");
        }
    }
}
