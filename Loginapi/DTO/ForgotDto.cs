using System.Text.Json.Serialization;

namespace Loginapi.DTO
{
    public class ForgotDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;
    }
}
