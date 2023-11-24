using IsometricShooterWebApp.Data.Models.Enums;

namespace IsometricShooterWebApp.Data.Models
{
    public class SeasonUserRewardModel
    {
        /// <summary>
        /// Saved offset in rating on <see cref="Date"/>
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Token count
        /// </summary>
        public double Count { get; set; }

        public bool Taken { get; set; }

        public virtual UserModel User { get; set; }

        public string UserId { get; set; }

        public virtual SeasonModel Season { get; set; }

        public StatisticsTypeEnum SeasonId { get; set; }

        public DateTime Date { get; set; }
    }
}
