namespace IsometricShooterWebApp.Data.Models
{
    public class UserGameLogModel
    {
        public Guid GameId { get; set; }

        public virtual GameModel Game { get; set; }

        public string UserId { get; set; }

        public virtual UserModel User { get; set; }

        public int Death { get; set; }

        public int Kills { get; set; }

        public double Money { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        public int AliveSecs { get; set; }
    }
}
