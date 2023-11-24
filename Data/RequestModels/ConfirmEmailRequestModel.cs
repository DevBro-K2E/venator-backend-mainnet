namespace IsometricShooterWebApp.Data.RequestModels
{
    public class ConfirmEmailRequestModel
    {
        public string UserId { get; set; }

        public string Code { get; set; }
    }
}
