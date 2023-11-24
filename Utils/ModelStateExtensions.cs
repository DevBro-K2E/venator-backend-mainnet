using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IsometricShooterWebApp.Utils
{
    public static class ModelStateExtensions
    {
        public static string GetFirstError(this ModelStateDictionary result)
        {
            return result.First(x=>x.Value.Errors.Any()).Value.Errors.First().ErrorMessage;
        }
    }
}
