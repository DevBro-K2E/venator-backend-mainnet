namespace IsometricShooterWebApp.Data.Models
{
    public class GameModel
    {
        public Guid Id { get; set; }

        public DateTime CreateTime { get; set; }

        public double Cost { get; set; }

        public string? WinnerUserId { get; set; }

        public bool IsFinished  { get; set; }

        public int RoundElapsedSecs { get; set; }

        public virtual UserModel? WinnerUser { get; set; } 

        public virtual List<GameMemberModel> Members { get; set; }

        public virtual List<UserGameLogModel> GameLogs { get; set; }
    }
}
