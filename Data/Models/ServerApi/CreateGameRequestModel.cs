using System.ComponentModel.DataAnnotations;

namespace IsometricShooterWebApp.Data.Models.ServerApi
{
    public class CreateGameRequestModel
    {
        [Required]
        public double? Cost { get; set; }

        public string[] UserIds { get; set; }
    }
}
