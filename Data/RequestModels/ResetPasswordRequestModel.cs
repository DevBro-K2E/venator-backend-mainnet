using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IsometricShooterWebApp.Data.RequestModels
{
    public class ResetPasswordRequestModel
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Code { get; set; }

        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }
    }
}
