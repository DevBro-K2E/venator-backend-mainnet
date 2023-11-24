using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace IsometricShooterWebApp.Utils
{
    public class GameApiAuthorizeAttribute : AuthorizeAttribute 
    {
        public const string Scheme = "GameApiAuth";

        public GameApiAuthorizeAttribute()
        {
            AuthenticationSchemes = Scheme;
        }
    }
}
