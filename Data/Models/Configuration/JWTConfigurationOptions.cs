using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IsometricShooterWebApp.Data.Models.Configuration
{
    public class JWTConfigurationOptions
    {
        public const string ConfigurationPath = "jwt";

        public string Secret { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }
        public int ExpDays { get; set; }

        public SymmetricSecurityKey GetSecutiryKey()
            => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
    }
}
