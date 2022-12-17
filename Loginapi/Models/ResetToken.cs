using System.ComponentModel.DataAnnotations;

namespace Loginapi.Models
{
    public class ResetToken
    {
        [Key()]
        public string Token { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
