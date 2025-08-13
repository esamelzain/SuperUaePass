using System.Text.Json.Serialization;

namespace SuperUaePass.Example.Web.Models
{
    public class UserVisit
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("pageUrl")]
        public string PageUrl { get; set; } = string.Empty;

        [JsonPropertyName("pageTitle")]
        public string PageTitle { get; set; } = string.Empty;

        [JsonPropertyName("userAgent")]
        public string UserAgent { get; set; } = string.Empty;

        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonPropertyName("referrer")]
        public string? Referrer { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("visitDuration")]
        public int? VisitDurationSeconds { get; set; }
    }
}
