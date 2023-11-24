namespace IsometricShooterWebApp.Data.Models.ServerApi
{
    public class CreateGameResponseModel
    {
        public Guid? GameId { get; set; }

        /// <summary>
        /// Broke if balance not more/equals needed
        /// </summary>
        public string[] BrokenUserIds { get; set; }
    }
}
