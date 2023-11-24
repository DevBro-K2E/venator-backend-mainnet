using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace IsometricShooterWebApp.Utils
{
    public class ServerApiAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            if (!context.HttpContext.Request.Headers.TryGetValue("key", out var key) || key.First() != configuration.GetValue("serverIdentityKey", "D2DB03CE-5EE4-430B-B0E3-707F02C92837"))
            {
                context.Result = new StatusCodeResult(404); 
                return Task.CompletedTask;
            }
            return next();
        }
    }
}
