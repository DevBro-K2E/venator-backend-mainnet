namespace IsometricShooterWebApp.Data.Models.GameApi
{
    public class GetRefereeRequestModel
    {
        public string Region { get; set; }

        public string AppVersion { get; set; }

        public string RoomName { get; set; }
    }
}
