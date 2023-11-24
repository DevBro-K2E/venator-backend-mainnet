using IsometricShooterWebApp.Data.Models.Enums;

namespace IsometricShooterWebApp.Data.Models
{
    public class SeasonUserModel
    {
        public int Points { get; set; }

        public virtual UserModel User { get; set; }

        public string UserId { get; set; }

        public virtual SeasonModel Season { get; set; }

        public StatisticsTypeEnum SeasonId { get; set; }
    }
}
