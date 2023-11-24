using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Data.Models.ServerApi;
using IsometricShooterWebApp.Managers;
using IsometricShooterWebApp.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IsometricShooterWebApp.Controllers
{
    [Route("serverapi")]
    [ServerApiAuthorize]
    public class ServerApiController : ControllerBase
    {
        private readonly UserManager<UserModel> userManager;
        private readonly ApplicationDbContext dbContext;
        private readonly AzureContainerInstancesManager azureContainerInstancesManager;
        private readonly ILogger<ServerApiController> logger;
        private readonly IConfiguration configuration;

        public ServerApiController(UserManager<UserModel> userManager, ApplicationDbContext dbContext, AzureContainerInstancesManager azureContainerInstancesManager, ILogger<ServerApiController> logger, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
            this.azureContainerInstancesManager = azureContainerInstancesManager;
            this.logger = logger;
            this.configuration = configuration;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequestModel query)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"{nameof(CreateGame)} request has model errors {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest(ModelState);
            }

            query.UserIds = query.UserIds.GroupBy(x => x).Select(x => x.Key).ToArray();

            var game = new GameModel()
            {
                CreateTime = DateTime.UtcNow,
                Cost = query.Cost.Value,
                Members = query.UserIds.Select(x => new GameMemberModel() { UserId = x }).ToList()
            };

            foreach (var item in game.Members)
            {
                item.User = await dbContext.Set<UserModel>().FindAsync(item.UserId);

                if (item.User == null)
                {
                    ModelState.AddModelError(nameof(CreateGameRequestModel.UserIds), $"user id = \"{item.UserId}\" not found");

                    logger.LogError($"{nameof(CreateGame)} request has invalid userId {item.UserId}");

                    return BadRequest(ModelState);
                }
            }

            if (query.Cost.HasValue && query.Cost > 0)
            {
                List<string> brokenUsers = new List<string>();

                var bcost = query.Cost.Value;

                bcost += (query.Cost.Value / 100) * configuration.GetValue<double>("game:commission");

                foreach (var item in game.Members)
                {
                    if (item.User.Balance < bcost)
                    {
                        brokenUsers.Add(item.UserId);
                    }

                    item.User.Balance -= bcost;
                }

                if (brokenUsers.Any())
                {
                    logger.LogError($"{nameof(CreateGame)} request has brokenUsers {JsonConvert.SerializeObject(brokenUsers)}");

                    return Ok(new CreateGameResponseModel()
                    {
                        GameId = default,
                        BrokenUserIds = brokenUsers.ToArray()
                    });
                }
            }

            game = dbContext.Set<GameModel>().Add(game).Entity;

            await dbContext.SaveChangesAsync();

            return Ok(new CreateGameResponseModel()
            {
                GameId = game.Id
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CalculateResult([FromBody] CalculateResultRequestModel query)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"{nameof(FinishGame)} request has model errors {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest(ModelState);
            }

            var game = await dbContext.Set<GameModel>()
                .FirstOrDefaultAsync(x => x.Id == query.GameId);

            if (game == default)
            {
                logger.LogError($"{nameof(FinishGame)} request has not found game with id {query.GameId}");

                return NotFound();
            }
            return Ok(CalculateResult(query.IsWinner, query.Stat, game.Cost));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> FinishPlayer([FromBody] FinishPlayerRequestModel query)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"{nameof(FinishPlayer)} request has model errors {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest(ModelState);
            }

            var game = await dbContext.Set<GameModel>()
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == query.GameId);

            if (game == default)
            {
                logger.LogError($"{nameof(FinishPlayer)} request has not found game with id {query.GameId}");

                return NotFound();
            }
            else if (game.IsFinished)
            {
                logger.LogError($"{nameof(FinishPlayer)} request has found game with id {query.GameId}, but finished - ignore");
                return Ok();
            }

            var moneyCount = CalculateResult(query.IsWinner, query.Stats, game.Cost);

            logger.LogInformation($"{nameof(FinishPlayer)} ({JsonConvert.SerializeObject(new { query.UserId, query.Stats, query.IsWinner, moneyCount })})");

            var gameLogSet = dbContext.Set<UserGameLogModel>();

            if (!await gameLogSet.AnyAsync(x => x.UserId == query.UserId && x.GameId == game.Id))
            {
                var item = game.Members.First(x => x.UserId == query.UserId);

                item.User = await dbContext.Set<UserModel>().FindAsync(item.UserId);

                ++item.User.GameCount;

                item.User.KillCount += query.Stats.KillCount;
                item.User.DeathCount += query.Stats.DeathCount;

                item.User.Balance += moneyCount;

                if (query.IsWinner)
                {
                    game.WinnerUserId = item.UserId;
                    game.WinnerUser = item.User;

                    ++item.User.WinCount;
                }

                gameLogSet.Add(new UserGameLogModel()
                {
                    GameId = game.Id,
                    Kills = query.Stats.KillCount,
                    Death = query.Stats.DeathCount,
                    Money = moneyCount * 1_000,
                    UserId = item.UserId,
                    AliveSecs = query.Stats.AliveSecs
                });

                foreach (var season in dbContext.Seasons.Select(x => x.Id).ToArray())
                {
                    var su = await dbContext.SeasonUsers.FindAsync(season, item.UserId);

                    if (su == null)
                    {
                        su = new SeasonUserModel() { SeasonId = season, UserId = item.UserId };

                        su = dbContext.SeasonUsers.Add(su).Entity;
                    }

                    su.Points += query.Stats.KillCount * 5;

                    if (query.IsWinner)
                        su.Points += 10;
                }

                await dbContext.SaveChangesAsync();
            }
            else
            {
                logger.LogInformation($"{nameof(FinishPlayer)} {query.UserId} duplicate query");
            }

            return Ok(moneyCount);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> FinishGame([FromBody] FinishGameRequestModel query)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"{nameof(FinishGame)} request has model errors {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest(ModelState);
            }

            if (!query.IsCancelled && query.UserWinnerId == default)
            {
                var errMessage = $"{nameof(FinishGameRequestModel.UserWinnerId)} must be set if '{nameof(FinishGameRequestModel.IsCancelled)}' equals 'false'";

                ModelState.AddModelError(nameof(FinishGameRequestModel.UserWinnerId), errMessage);

                logger.LogError($"{nameof(FinishGame)} request has error {errMessage}");

                return BadRequest(ModelState);
            }

            var game = await dbContext.Set<GameModel>()
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == query.GameId);

            if (game == default)
            {
                logger.LogError($"{nameof(FinishGame)} request has not found game with id {query.GameId}");

                return NotFound();
            }
            else if (game.IsFinished)
            {
                logger.LogError($"{nameof(FinishGame)} request has found game with id {query.GameId}, but finished - ignore");
                return Ok();
            }

            game.IsFinished = true;
            game.RoundElapsedSecs = query.RoundElapsedSecs;

            foreach (var item in game.Members)
            {
                item.User = await dbContext.Set<UserModel>().FindAsync(item.UserId);
            }

            if (query.IsCancelled)
            {
                foreach (var item in game.Members)
                {
                    item.User.Balance += game.Cost;
                }
            }

            await dbContext.SaveChangesAsync();

            //todo: destroy room
            if (!string.IsNullOrWhiteSpace(query.RoomName))
                await azureContainerInstancesManager.DestroyContainerAsync(query.RoomName);

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DestroyRoom([FromBody] string roomName)
        {
            await azureContainerInstancesManager.DestroyContainerAsync(roomName);

            return Ok();
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> CreateLocalGame([FromBody] CreateGameRequestModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var game = new GameModel()
            {
                CreateTime = DateTime.UtcNow,
                Members = query.UserIds.Select(x => new GameMemberModel() { UserId = x }).ToList()
            };

            foreach (var item in game.Members)
            {
                item.User = await dbContext.Set<UserModel>().FindAsync(item.UserId);

                if (item.User == null)
                {
                    ModelState.AddModelError(nameof(CreateGameRequestModel.UserIds), $"user id = \"{item.UserId}\" not found");

                    return BadRequest(ModelState);
                }
            }

            game = dbContext.Set<GameModel>().Add(game).Entity;

            await dbContext.SaveChangesAsync();

            return Ok(new CreateGameResponseModel()
            {
                GameId = game.Id
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> FinishLocalGame([FromBody] FinishLocalGameRequestModel query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (query.UserWinnerId == default)
            {
                ModelState.AddModelError(nameof(FinishLocalGameRequestModel.UserWinnerId), $"{nameof(FinishLocalGameRequestModel.UserWinnerId)} must be set");
                return BadRequest(ModelState);
            }

            var game = await dbContext.Games.FindAsync(query.GameId);

            if (game == default)
                return BadRequest($"Error!! Game with GameId = \"{query.GameId}\" not found");

            if (game.Cost != default)
                return BadRequest($"Error!! Game with GameId = \"{query.GameId}\" is not be local game");

            if (!await dbContext.GameMembers.AnyAsync(x => x.GameId == query.GameId && x.UserId == query.UserWinnerId))
                return BadRequest($"Error!! Member in game with GameId = \"{query.GameId}\" and UserWinnerId = \"{query.UserWinnerId}\" not found");

            var member = await dbContext.Users.FindAsync(query.UserWinnerId);

            if (member == null)
                return BadRequest($"Error!! Member with UserWinnerId = \"{query.UserWinnerId}\" not found");

            ++member.GameCount;

            member.KillCount += query.Stat.KillCount;
            member.DeathCount += query.Stat.DeathCount;


            var moneyCount = 0;

            if (query.IsWinner == true || query.UserId == default)
            {
                moneyCount = (query.Stat.KillCount * 1);

                member.SilverCoins += moneyCount;
            }

            ++member.WinCount;

            await dbContext.SaveChangesAsync();

            //todo: destroy room
            //azureContainerInstancesManager.DestroyContainerAsync()

            return Ok(moneyCount);
        }

        private double CalculateResult(bool winner, FinishGameMemberStatModel stat, double cost)
        {
            if (cost == 0)
                return 0.0;

            var result = stat.KillCount * cost;

            if (winner)
                result += cost;

            return result;
        }
    }
}
