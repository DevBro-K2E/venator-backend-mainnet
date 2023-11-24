namespace IsometricShooterWebApp.Data.Models
{
    public class UserSessionModel
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public string IpAddress { get; set; }

        public string UserId { get; set; }

        public virtual UserModel User { get; set; }

    }
}
