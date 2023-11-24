using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IsometricShooterWebApp.Data.RequestModels
{
    public class RegisterRequestModel
    {
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        public string Password { get; set; }

        [JsonPropertyName("invite_token")]
        public string? InviteToken { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get;set; }

        [JsonPropertyName("return_url")]
        public string? ReturnUrl { get;set; }
    }
}
