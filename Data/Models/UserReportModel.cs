namespace IsometricShooterWebApp.Data.Models
{
    public class UserReportModel
    {
        public Guid Id { get; set; }

        public string CreatorId { get; set; }

        public virtual UserModel Creator { get; set; }

        public string TargetId { get; set; }

        public virtual UserModel Target { get; set; }

        public string Message { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
