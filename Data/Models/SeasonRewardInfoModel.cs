using IsometricShooterWebApp.Data.Models.Enums;

namespace IsometricShooterWebApp.Data.Models
{
    public class SeasonRewardInfoModel
    {
        public double Count { get; set; }

        public int EndOffset { get; set; }

        public virtual SeasonModel Season { get; set; }

        public StatisticsTypeEnum SeasonId { get; set; }
    }
}
