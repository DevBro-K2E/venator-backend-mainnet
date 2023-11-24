using System.ComponentModel.DataAnnotations;

namespace IsometricShooterWebApp.Data.RequestModels
{
    public class WithdrawalRequestModel
    {
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Count")]
        public double Count { get; set; }
    }
}
