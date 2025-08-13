using System.Text.Json.Serialization;

namespace SuperUaePass.Example.Web.Models
{
    public class DataCollectionRoot
    {
        [JsonPropertyName("userVisits")]
        public List<UserVisit> UserVisits { get; set; } = new();

        [JsonPropertyName("pageFeedback")]
        public List<PageFeedback> PageFeedback { get; set; } = new();

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
