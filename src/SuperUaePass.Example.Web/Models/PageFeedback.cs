using System.Text.Json.Serialization;

namespace SuperUaePass.Example.Web.Models
{
    public class PageFeedback
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("pageUrl")]
        public string PageUrl { get; set; } = string.Empty;

        [JsonPropertyName("pageTitle")]
        public string PageTitle { get; set; } = string.Empty;

        [JsonPropertyName("wasHelpful")]
        public bool WasHelpful { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("userAgent")]
        public string UserAgent { get; set; } = string.Empty;

        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;
    }
}
