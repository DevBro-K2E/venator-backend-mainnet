using IsometricShooterWebApp.Data;
using IsometricShooterWebApp.Data.Models;
using IsometricShooterWebApp.Data.Models.Configuration;
using IsometricShooterWebApp.Data.Models.GameApi;
using IsometricShooterWebApp.Data.RequestModels;
using IsometricShooterWebApp.Managers;
using IsometricShooterWebApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace IsometricShooterWebApp.Controllers
{
    /// <summary>
    /// Game api for calling for unity client
    /// </summary>
    [Route("gameapi")]
    [GameApiAuthorize]
    public class GameApiController : ControllerBase
    {
        private readonly SignInManager<UserModel> signInManager;
        private readonly ApplicationDbContext dbContext;
        private readonly RefereeManager refereeManager;
        private readonly UserManager userManager;
        private readonly SeasonManager seasonManager;
        private readonly IConfiguration configuration;

        public GameApiController(
            SignInManager<UserModel> signInManager,
            ApplicationDbContext dbContext,
            RefereeManager refereeManager,
            UserManager userManager,
            SeasonManager seasonManager,
            IConfiguration configuration)
        {
            this.signInManager = signInManager;
            this.dbContext = dbContext;
            this.refereeManager = refereeManager;
            this.userManager = userManager;
            this.seasonManager = seasonManager;
            this.configuration = configuration;
        }

        /// <summary>
        /// Authorize account with loginName and password
        /// </summary>
        /// <param name="query"></param>
        /// <response code="200">Return { Result:bool, AuthorizeHeader:string }</response>
        /// <returns></returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel query)
            => Ok(await userManager.Login(signInManager, query));

        /// <summary>
        /// Return information about current authorized account
        /// </summary>
        /// <response code="200">Return { Balance:double, KillCount:int, DeathCount:int, GameCount:int, WinCount:int, CreationDate:DateTime }</response>
        /// <returns></returns>
        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUserInfo()
            => Ok(await userManager.GetCurrentUserInfo(signInManager.UserManager, User));

        /// <summary>
        /// Change name for authorized account
        /// </summary>
        /// <response code="200"></response>
        /// <returns></returns>
        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeUserName([FromBody] string name)
            => await userManager.ChangeName(signInManager.UserManager, User, ModelState, name) ? Ok() : BadRequest(ModelState);

        /// <summary>
        /// 
        /// </summary>
        /// <response code="200"></response>
        /// <response code="500"></response>
        /// <returns></returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReferee([FromBody] GetRefereeRequestModel query)
            => await refereeManager.RunReferee(query) ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);

        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateReport([FromBody] UserReportRequestModel query)
            => await userManager.CreateReport(dbContext, signInManager.UserManager, User, query) ? Ok() : BadRequest();

        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics([FromBody] GetStatisticsRequestModel query)
            => Ok(await userManager.GetStatistics(dbContext, query.StatisticsType));

        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSeasonStatistics([FromBody] GetStatisticsRequestModel query)
            => Ok(await seasonManager.GetStatistics(dbContext, signInManager.UserManager, User, query.StatisticsType));

        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSeasonRewards()
            => Ok(await seasonManager.GetRewards(dbContext, signInManager.UserManager, User));

        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TakeSeasonReward([FromBody] TakeSeasonRewardRequestModel query)
        { 
            var money = await seasonManager.TakeReward(dbContext, signInManager.UserManager, User, query); 
            return money == null ? BadRequest() : Ok(money); 
        }

        [HttpPost("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentCommission()
            => Ok(configuration.GetValue<double>("game:commission"));
    }
}
