using IsometricShooterWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsometricShooterWebApp.Controllers.Front
{
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public PublicController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("winners")]
        public async Task<IActionResult> Winners()
        {
            return Ok(new
            {
                TopWinners = await dbContext.GameLogs
                .Include(x => x.Game)
                .Include(x => x.User)
                .OrderByDescending(x => x.Kills)
                .Where(x => x.Game.WinnerUserId == x.UserId)
                .Take(10)
                .Select(x => new { x.User.Name, x.Kills, x.CreateTime })
                .ToArrayAsync()
            });
        }

        [HttpPost("leaderboard")]
        public async Task<IActionResult> Leaderboard()
        {
            return Ok(new
            {
                Leaderboard = await dbContext.Users
                .OrderByDescending(x => 1.0 * x.KillCount / (x.DeathCount + 1))
                .Take(10)
                .Select(x => new { x.Name, KDRatio = 1.0 * x.KillCount / (x.DeathCount + 1) })
                .ToArrayAsync()
            });
        }
    }
}
