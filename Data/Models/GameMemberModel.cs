using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IsometricShooterWebApp.Data.Models
{
    public class GameMemberModel
    {
        public Guid GameId { get; set; }

        public virtual GameModel Game { get; set; }

        public string UserId { get; set; }

        public virtual UserModel User { get; set; }
    }
}
