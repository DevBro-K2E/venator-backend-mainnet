using System.Text.Json.Serialization;

namespace IsometricShooterWebApp.Data.RequestModels
{
    public class PasswordChangeRequestModel
    {
        [JsonPropertyName("current_password")]
        public string CurrentPassword { get; set; }

        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }
    }
}
