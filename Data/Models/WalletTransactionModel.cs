namespace IsometricShooterWebApp.Data.Models
{
    public class WalletTransactionModel
    {
        public Guid Id { get; set; }

        public double Amount { get; set; }

        public string TransactionId { get; set; }

        public DateTime CreateTime { get; set; }

        public bool Accepted { get; set; }

        public string UserId  { get; set; }

        public virtual UserModel User { get; set; }
    }
}
