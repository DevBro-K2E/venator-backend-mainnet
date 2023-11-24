namespace IsometricShooterWebApp.Data.Models.GameApi
{
    public class StatisticsItem
    {
        public string UserId { get; set; }

        public int TotalKills { get; set; }

        public int TotalDeath { get; set; }

        public double TotalMoney { get; set; }
    }
}
