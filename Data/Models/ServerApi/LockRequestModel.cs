namespace IsometricShooterWebApp.Data.Models.ServerApi
{
    public class UnLockRequestModel
    {
        public string UserId { get; set; }
    }

    public class LockRequestModel : UnLockRequestModel
    {
        public double Cost { get; set; }
    }
}
