using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IsometricShooterWebApp.Data.RequestModels
{
    public class ResetPasswordQueryRequestModel
    {
        [EmailAddress]
        public string Email { get; set; }

        [JsonPropertyName("return_url")]
        public string? ReturnUrl { get; set; }
    }
}
