using System.ComponentModel.DataAnnotations;

namespace IsometricShooterWebApp.Data.Models.ServerApi
{
    public class FinishLocalGameRequestModel
    {
        private bool? isWinner;

        [Required]
        public Guid? GameId { get; set; }

        [Obsolete]
        public string? UserWinnerId { get; set; }

        public string? UserId { get; set; }

        public bool? IsWinner { get { return isWinner == true || UserId == UserWinnerId; } set { isWinner = value; UserWinnerId = UserId; } }

        public FinishGameMemberStatModel? Stat { get; set; }
    }
}
