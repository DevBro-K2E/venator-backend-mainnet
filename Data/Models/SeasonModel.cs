using IsometricShooterWebApp.Data.Models.Enums;
using NodaTime;

namespace IsometricShooterWebApp.Data.Models
{
    public class SeasonModel
    {
        public StatisticsTypeEnum Id { get; set; }

        public NodaTime.Period SeasonOffset { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public virtual List<SeasonRewardInfoModel> RewardInfo { get; set; }

        public virtual List<SeasonUserRewardModel> UserRewards { get; set; }

        public virtual List<SeasonUserModel> Users { get; set; }
    }
}
