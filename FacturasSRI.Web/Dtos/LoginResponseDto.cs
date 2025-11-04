using System.Text.Json.Serialization;

namespace FacturasSRI.Web.Dtos
{
    public class LoginResponseDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}