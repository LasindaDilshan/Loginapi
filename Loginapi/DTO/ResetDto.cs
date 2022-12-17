using System.Text.Json.Serialization;

namespace Loginapi.DTO
{
    public class ResetDto
    {
        
        [JsonPropertyName("token")]
        public string Token { get; set; } = default!;
        [JsonPropertyName("password")]
        public string Password { get; set; } = default!;
        [JsonPropertyName("PasswordConfirm")]
        public string PasswordConfirm { get; set; } = default!;
    }
}
