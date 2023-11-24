using System.ComponentModel.DataAnnotations;

namespace IsometricShooterWebApp.Data.Models
{
    public class WalletBlockModel
    {
        [Key]
        public long BlockId { get; set; }

        public int ReceiveCount { get; set; }
    }
}
