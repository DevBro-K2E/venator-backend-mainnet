using Microsoft.AspNetCore.Identity;

namespace IsometricShooterWebApp.Data.Models
{
    public class UserModel : IdentityUser
    {
        public double Balance { get; set; }

        public double Money { get => Balance * 1000; set => Balance = value / 1000; }

        public double SilverCoins { get; set; } = 0;

        public int KillCount { get; set; }

        public int DeathCount { get; set; }

        public int GameCount { get; set; }

        public int WinCount { get; set; }

        public DateTime CreationDate { get; set; }

        public virtual List<UserSessionModel> Sessions { get; set; }

        public int InvitationRedirectCount { get; set; }

        public int InvitationRegistrationCount { get; set; }

        public virtual List<GameModel> Games { get; set; }

        public virtual List<UserGameLogModel> GameLogs { get; set; }


        public virtual List<SeasonUserModel> SeasonUser { get; set; }

        public string? Name { get; set; }
    }
}
