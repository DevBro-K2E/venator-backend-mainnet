using Dapper;
using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Data.Models.Enums;
using IsometricShooterWebApp.Data.RequestModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace IsometricShooterWebApp.Managers
{
    public class SeasonManager
    {

        public SeasonManager(IServiceProvider serviceProvider, ILogger<SeasonManager> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<SeasonManager> logger;
        Timer checkSeasonsTimer;

        public void Initialize()
        {
            InitCheckSeasonsTimer(TimeSpan.FromSeconds(2));
        }

        private void InitCheckSeasonsTimer(TimeSpan delay)
        {
            checkSeasonsTimer = new Timer(e => CheckSeasons(), null, delay, Timeout.InfiniteTimeSpan);
        }

        private async void CheckSeasons()
        {
            var now = DateTime.UtcNow;

            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();

                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var seasons = await db.Seasons
                    .Include(x => x.RewardInfo)
                    .ToArrayAsync();


                foreach (var season in seasons)
                {
                    season.StartTime = season.StartTime.ToUniversalTime();
                    season.EndTime = season.EndTime.ToUniversalTime();

                    if (season.EndTime > now)
                        continue;

                    var eTime = season.EndTime;

                    season.StartTime = season.EndTime;
                    season.EndTime = season.EndTime.AddDays(season.SeasonOffset.Days);

                    for (int i = 0; i < season.SeasonOffset.Months; i++)
                    {
                        season.EndTime = season.EndTime.AddDays(DateTime.DaysInMonth(season.EndTime.Year, season.EndTime.Month));
                    }

                    if (!season.RewardInfo.Any())
                        throw new Exception($"Not have any reward items");

                    var maxTop = season.RewardInfo.Max(x => x.EndOffset);

                    using var transaction = await db.Database.BeginTransactionAsync();

                    var userIds = await db.SeasonUsers
                        .Where(x => x.SeasonId == season.Id)
                        .OrderByDescending(x => x.Points)
                        .Take(maxTop)
                        .Select(x => x.UserId)
                        .ToArrayAsync();

                    int latestOffset = 0;

                    foreach (var reward in season.RewardInfo.OrderBy(x => x.EndOffset).ToArray())
                    {
                        for (int i = latestOffset; i < reward.EndOffset && i < userIds.Length; i++)
                        {
                            var uid = userIds[i];

                            db.SeasonUserRewards.Add(new SeasonUserRewardModel()
                            {
                                SeasonId = season.Id,
                                UserId = uid,
                                Count = reward.Count,
                                Offset = i + 1,
                                Date = eTime
                            });
                        }

                        latestOffset = reward.EndOffset;
                    }

                    db.SeasonUsers.RemoveRange(db.SeasonUsers
                        .Where(x => x.SeasonId == season.Id));

                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, ex.ToString());
            }

            InitCheckSeasonsTimer(now.Date.AddDays(1).AddMinutes(1) - DateTime.UtcNow);
        }


        internal async Task<object?> GetStatistics(ApplicationDbContext dbContext, UserManager<UserModel> userManager, ClaimsPrincipal user, StatisticsTypeEnum statisticsType)
        {
            var uid = userManager.GetUserId(user);

            var season = await dbContext.Seasons.FindAsync(statisticsType);

            if (season == null)
                return null;

            var statistics = (await dbContext.Database.GetDbConnection().QueryAsync($"SELECT ROW_NUMBER() OVER (ORDER BY t.\"Points\" DESC) AS Pos, t.\"UserId\", a.\"Name\", t.\"Points\", (\r\n    SELECT COALESCE(SUM(g.\"Kills\"), 0)::INT\r\n    FROM \"GameLogs\" AS g\r\n    WHERE (a.\"Id\" = g.\"UserId\") AND ((g.\"CreateTime\" > s0.\"StartTime\") AND (g.\"CreateTime\" < s0.\"EndTime\"))) AS \"totalKills\", (\r\n    SELECT COALESCE(SUM(g0.\"Death\"), 0)::INT\r\n    FROM \"GameLogs\" AS g0\r\n    WHERE (a.\"Id\" = g0.\"UserId\") AND ((g0.\"CreateTime\" > s0.\"StartTime\") AND (g0.\"CreateTime\" < s0.\"EndTime\"))) AS \"totalDeath\", (\r\n    SELECT COALESCE(SUM(g1.\"Money\"), 0.0)\r\n    FROM \"GameLogs\" AS g1\r\n    WHERE (a.\"Id\" = g1.\"UserId\") AND ((g1.\"CreateTime\" > s0.\"StartTime\") AND (g1.\"CreateTime\" < s0.\"EndTime\"))) AS \"totalMoney\", (\r\n    SELECT COUNT(*)::INT\r\n    FROM \"GameLogs\" AS g2\r\n    WHERE (a.\"Id\" = g2.\"UserId\") AND ((g2.\"CreateTime\" > s0.\"StartTime\") AND (g2.\"CreateTime\" < s0.\"EndTime\"))) AS \"gameCount\", (\r\n    SELECT COUNT(*)::INT\r\n    FROM \"Games\" AS g3\r\n    WHERE (a.\"Id\" = g3.\"WinnerUserId\") AND ((g3.\"CreateTime\" > s0.\"StartTime\") AND (g3.\"CreateTime\" < s0.\"EndTime\"))) as \"winGameCount\"\r\nFROM (\r\n    SELECT s.\"SeasonId\", s.\"UserId\", s.\"Points\"\r\n    FROM \"SeasonUsers\" AS s\r\n    WHERE s.\"SeasonId\" = @__statisticsType_0\r\n    ORDER BY s.\"Points\" DESC\r\n    LIMIT @__p_1\r\n) AS t\r\nINNER JOIN \"AspNetUsers\" AS a ON t.\"UserId\" = a.\"Id\"\r\nINNER JOIN \"Seasons\" AS s0 ON t.\"SeasonId\" = s0.\"Id\"\r\nORDER BY t.\"Points\" DESC", new { __statisticsType_0 = statisticsType, __p_1 = 20 }))
                .Select(x => new
                {
                    x.UserId,
                    x.Name,
                    x.Points,
                    x.totalKills,
                    x.totalDeath,
                    x.totalMoney,
                    x.gameCount,
                    GameVictory = x.winGameCount,
                    x.pos
                }).ToList();

            var playerStatistics = (await dbContext.Database.GetDbConnection().QueryAsync($"SELECT q.* \r\nFROM (\r\nSELECT ROW_NUMBER() OVER (ORDER BY t.\"Points\" DESC) AS Pos, t.\"UserId\", a.\"Name\", t.\"Points\", (\r\n    SELECT COALESCE(SUM(g.\"Kills\"), 0)::INT\r\n    FROM \"GameLogs\" AS g\r\n    WHERE (a.\"Id\" = g.\"UserId\") AND ((g.\"CreateTime\" > s0.\"StartTime\") AND (g.\"CreateTime\" < s0.\"EndTime\"))) AS \"totalKills\", (\r\n    SELECT COALESCE(SUM(g0.\"Death\"), 0)::INT\r\n    FROM \"GameLogs\" AS g0\r\n    WHERE (a.\"Id\" = g0.\"UserId\") AND ((g0.\"CreateTime\" > s0.\"StartTime\") AND (g0.\"CreateTime\" < s0.\"EndTime\"))) AS \"totalDeath\", (\r\n    SELECT COALESCE(SUM(g1.\"Money\"), 0.0)\r\n    FROM \"GameLogs\" AS g1\r\n    WHERE (a.\"Id\" = g1.\"UserId\") AND ((g1.\"CreateTime\" > s0.\"StartTime\") AND (g1.\"CreateTime\" < s0.\"EndTime\"))) AS \"totalMoney\", (\r\n    SELECT COUNT(*)::INT\r\n    FROM \"GameLogs\" AS g2\r\n    WHERE (a.\"Id\" = g2.\"UserId\") AND ((g2.\"CreateTime\" > s0.\"StartTime\") AND (g2.\"CreateTime\" < s0.\"EndTime\"))) AS \"gameCount\", (\r\n    SELECT COUNT(*)::INT\r\n    FROM \"Games\" AS g3\r\n    WHERE (a.\"Id\" = g3.\"WinnerUserId\") AND ((g3.\"CreateTime\" > s0.\"StartTime\") AND (g3.\"CreateTime\" < s0.\"EndTime\"))) as \"winGameCount\"\r\nFROM (\r\n    SELECT s.\"SeasonId\", s.\"UserId\", s.\"Points\"\r\n    FROM \"SeasonUsers\" AS s\r\n    WHERE s.\"SeasonId\" = @__statisticsType_0\r\n    ORDER BY s.\"Points\" DESC\r\n) AS t\r\nINNER JOIN \"AspNetUsers\" AS a ON t.\"UserId\" = a.\"Id\"\r\nINNER JOIN \"Seasons\" AS s0 ON t.\"SeasonId\" = s0.\"Id\"\r\n\t) as q\r\n\tWHERE q.\"UserId\" like @__uid\r\n", new { __statisticsType_0 = statisticsType, __uid = uid }))
                .Select(x => new
                {
                    x.UserId,
                    x.Name,
                    x.Points,
                    x.totalKills,
                    x.totalDeath,
                    x.totalMoney,
                    x.gameCount,
                    GameVictory = x.winGameCount,
                    x.pos
                }).FirstOrDefault();

            var seasonRewards = await dbContext.SeasonRewards
                .Where(x => x.SeasonId == statisticsType)
                .Select(x => new { x.EndOffset, x.Count })
                .ToArrayAsync();

            return new { season = new { EndTime = season.EndTime.ToUniversalTime(), season.Id }, seasonRewards, statistics, playerStatistics };
        }

        internal async Task<object?> GetRewards(ApplicationDbContext dbContext, UserManager<UserModel> userManager, ClaimsPrincipal user)
        {
            var uid = userManager.GetUserId(user);

            var r = await dbContext.SeasonUserRewards
                .Where(x => x.UserId == uid && !x.Taken)
                .Select(x => new { x.SeasonId, x.Count, x.Offset, x.Date })
                .ToArrayAsync();

            return r;
        }

        internal async Task<object?> TakeReward(ApplicationDbContext dbContext, UserManager<UserModel> userManager, ClaimsPrincipal user, TakeSeasonRewardRequestModel query)
        {
            var uid = userManager.GetUserId(user);

            var reward = await dbContext.SeasonUserRewards.FindAsync(query.Season, uid, query.Date);

            if (reward == null || reward.Taken)
                return null;

            var u = await dbContext.Users.FindAsync(uid);

            u.Money += reward.Count;

            reward.Taken = true;

            await dbContext.SaveChangesAsync();

            return u.Money;
        }
    }
}
