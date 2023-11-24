using System.ComponentModel.DataAnnotations;

namespace IsometricShooterWebApp.Data.Models.ServerApi
{
    public class CalculateResultRequestModel
    {
        [Required]
        public Guid? GameId { get; set; }

        public bool IsWinner { get; set; }

        public FinishGameMemberStatModel Stat { get; set; }
    }
}
