using IsometricShooterWebApp.Data.Models.Enums;

namespace IsometricShooterWebApp.Data.RequestModels
{
    public class TakeSeasonRewardRequestModel
    {
        public StatisticsTypeEnum Season { get; set; }

        public DateTime Date { get; set; }
    }
}
