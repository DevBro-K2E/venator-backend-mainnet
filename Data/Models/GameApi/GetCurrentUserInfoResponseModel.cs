using System;
using System.Collections;
using System.Collections.Generic;

namespace IsometricShooterWebApp.Data.Models.GameApi
{
    public class GetCurrentUserInfoResponseModel
    {
        public string Id { get; set; }

        public double Balance { get; set; }

        public double GameCoinsBalance { get; set; }

        public int KillCount { get; set; }

        public int DeathCount { get; set; }

        public int GameCount { get; set; }

        public int WinCount { get; set; }

        public DateTime CreationDate { get; set; }

        public string Name { get; set; }
    }
}