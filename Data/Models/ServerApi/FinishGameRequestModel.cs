using System.ComponentModel.DataAnnotations;

namespace IsometricShooterWebApp.Data.Models.ServerApi
{
    public class FinishPlayerRequestModel
    {
        [Required]
        public Guid? GameId { get; set; }

        public bool IsWinner { get; set; }

        public FinishGameMemberStatModel? Stats { get; set; }

        public string UserId { get; set; }
    }

    public class FinishGameRequestModel
    {
        [Required]
        public Guid? GameId { get; set; }

        public string RoomName { get; set; }

        /// <summary>
        /// if need cancel the game and return cost
        /// </summary>
        public bool IsCancelled { get; set; } = false;

        public string? UserWinnerId { get; set; }

        public Dictionary<string, FinishGameMemberStatModel>? StatMap { get; set; }

        public int RoundElapsedSecs { get; set; }
    }
}
