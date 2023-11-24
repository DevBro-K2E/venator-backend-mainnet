using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Data.Models.Configuration;
using IsometricShooterWebApp.Data.Models.Enums;
using IsometricShooterWebApp.Data.Models.GameApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IsometricShooterWebApp.Managers
{
    public class UserManager
    {
        private readonly IOptions<JWTConfigurationOptions> jwtConfigurationOptions;

        public UserManager(IOptions<JWTConfigurationOptions> JWTConfigurationOptions)
        {
            jwtConfigurationOptions = JWTConfigurationOptions;
        }

        internal async Task<bool> CreateReport(ApplicationDbContext dbContext, UserManager<UserModel> userManager, ClaimsPrincipal user, UserReportRequestModel query)
        {
            dbContext.UserReports.Add(new UserReportModel() { CreatorId = userManager.GetUserId(user), TargetId = query.TargetId, Message = query.Message, CreateTime = DateTime.UtcNow });

            await dbContext.SaveChangesAsync();

            return true;
        }

        internal async Task<GetCurrentUserInfoResponseModel> GetCurrentUserInfo(UserManager<UserModel> userManager, ClaimsPrincipal user)
        {
            var u = await userManager.GetUserAsync(user);

            return new GetCurrentUserInfoResponseModel
            {
                Id = u.Id,

                Balance = u.Money,

                GameCoinsBalance = u.SilverCoins,

                KillCount = u.KillCount,

                DeathCount = u.DeathCount,

                GameCount = u.GameCount,

                WinCount = u.WinCount,

                CreationDate = u.CreationDate,

                Name = u.Name
            };
        }

        internal async Task<object?> GetStatistics(ApplicationDbContext dbContext, StatisticsTypeEnum statisticsType)
        {
            DateTime startTime = default;
            DateTime endTime = default;

            switch (statisticsType)
            {
                case StatisticsTypeEnum.Day:
                    startTime = DateTime.Today;
                    endTime = startTime.AddDays(1);
                    break;
                case StatisticsTypeEnum.Week:
                    startTime = DateTime.Today.AddDays(1 -(int)DateTime.Today.DayOfWeek);
                    endTime = startTime.AddDays(7);
                    break;
                case StatisticsTypeEnum.Month:
                    startTime = DateTime.Today.AddDays(1 - (DateTime.Today.Day));
                    endTime = startTime.AddDays(DateTime.DaysInMonth(startTime.Year, startTime.Month));
                    break;
                default:
                    break;
            }
            startTime = startTime.AddSeconds(1);
            endTime = endTime.AddSeconds(1);

            var logs = await dbContext.GameLogs
                .Where(x => x.CreateTime > startTime && x.CreateTime < endTime)
                .GroupBy(x => x.UserId)
                .Select(x => new { userId = x.Key, totalKills = x.Sum(b => b.Kills), totalDeath = x.Sum(x => x.Death), totalMoney = x.Sum(b => b.Money), GameCount = x.Count() })
                .OrderByDescending(x=>x.totalKills / (x.totalDeath + 1d))
                .Take(100)
                .ToArrayAsync();

            return logs;
        }

        internal async Task<LoginResponseModel> Login(SignInManager<UserModel> signInManager, LoginRequestModel query)
        {
            var user = await signInManager.UserManager.FindByNameAsync(query.LoginName);

            if (user == default)
                return new LoginResponseModel() { Result = false };

            var signResult = await signInManager.CheckPasswordSignInAsync(user, query.Password, false);

            if (!signResult.Succeeded)
                return new LoginResponseModel() { Result = false };

            return new LoginResponseModel() { Result = true, AuthorizeHeader = BuildToken(user) };
        }

        public string BuildToken(UserModel user)
        {
            var claims = new[] {
            new Claim(ClaimTypes.Name, user.UserName),
                //new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.NameIdentifier,
                user.Id)
            };

            var credentials = new SigningCredentials(jwtConfigurationOptions.Value.GetSecutiryKey(), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(jwtConfigurationOptions.Value.Issuer, jwtConfigurationOptions.Value.Audience, claims,
                expires: DateTime.Now.AddDays(jwtConfigurationOptions.Value.ExpDays), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        internal async Task<bool> ChangeName(UserManager<UserModel> userManager, ClaimsPrincipal user, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState, string name)
        {
            var u = await userManager.GetUserAsync(user);

            u.Name = name;

            await userManager.UpdateAsync(u);

            return true;
        }
    }
}
