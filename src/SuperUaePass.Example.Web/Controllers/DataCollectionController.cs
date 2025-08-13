using Microsoft.AspNetCore.Mvc;
using SuperUaePass.Example.Web.Models;
using System.Text.Json;

namespace SuperUaePass.Example.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataCollectionController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DataCollectionController> _logger;
        private readonly string _dataFilePath;

        public DataCollectionController(IWebHostEnvironment environment, ILogger<DataCollectionController> logger)
        {
            _environment = environment;
            _logger = logger;
            _dataFilePath = Path.Combine(_environment.WebRootPath, "data-collection", "user-data.json");
        }

        [HttpPost("visit")]
        public async Task<IActionResult> RecordVisit([FromBody] UserVisit visit)
        {
            try
            {
                // Set additional data from request context
                visit.IpAddress = GetClientIpAddress();
                visit.UserAgent = Request.Headers.UserAgent.ToString();
                visit.Referrer = Request.Headers.Referer.ToString();
                visit.SessionId = HttpContext.Session.Id;

                await AppendToJsonFile(visit, "userVisits");
                
                _logger.LogInformation("User visit recorded for page: {PageUrl}", visit.PageUrl);
                return Ok(new { success = true, message = "Visit recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording user visit");
                return StatusCode(500, new { success = false, message = "Failed to record visit" });
            }
        }

        [HttpPost("feedback")]
        public async Task<IActionResult> RecordFeedback([FromBody] PageFeedback feedback)
        {
            try
            {
                // Set additional data from request context
                feedback.IpAddress = GetClientIpAddress();
                feedback.UserAgent = Request.Headers.UserAgent.ToString();
                feedback.SessionId = HttpContext.Session.Id;

                await AppendToJsonFile(feedback, "pageFeedback");
                
                _logger.LogInformation("Page feedback recorded for page: {PageUrl}, Helpful: {WasHelpful}", 
                    feedback.PageUrl, feedback.WasHelpful);
                return Ok(new { success = true, message = "Feedback recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording page feedback");
                return StatusCode(500, new { success = false, message = "Failed to record feedback" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var data = await LoadDataFromJson();
                
                var stats = new
                {
                    totalVisits = data.UserVisits.Count,
                    totalFeedback = data.PageFeedback.Count,
                    helpfulFeedback = data.PageFeedback.Count(f => f.WasHelpful),
                    notHelpfulFeedback = data.PageFeedback.Count(f => !f.WasHelpful),
                    lastUpdated = data.LastUpdated
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats");
                return StatusCode(500, new { success = false, message = "Failed to retrieve stats" });
            }
        }

        [HttpGet("raw-data")]
        public async Task<IActionResult> GetRawData()
        {
            try
            {
                var data = await LoadDataFromJson();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving raw data");
                return StatusCode(500, new { success = false, message = "Failed to retrieve raw data" });
            }
        }

        private async Task AppendToJsonFile<T>(T item, string collectionName) where T : class
        {
            var data = await LoadDataFromJson();
            
            // Add item to appropriate collection
            if (collectionName == "userVisits" && item is UserVisit visit)
            {
                data.UserVisits.Add(visit);
            }
            else if (collectionName == "pageFeedback" && item is PageFeedback feedback)
            {
                data.PageFeedback.Add(feedback);
            }

            data.LastUpdated = DateTime.UtcNow;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_dataFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            // Write back to file
            var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await System.IO.File.WriteAllTextAsync(_dataFilePath, jsonString);
        }

        private async Task<DataCollectionRoot> LoadDataFromJson()
        {
            if (!System.IO.File.Exists(_dataFilePath))
            {
                return new DataCollectionRoot();
            }

            var jsonString = await System.IO.File.ReadAllTextAsync(_dataFilePath);
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new DataCollectionRoot();
            }

            try
            {
                return JsonSerializer.Deserialize<DataCollectionRoot>(jsonString, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new DataCollectionRoot();
            }
            catch (JsonException)
            {
                _logger.LogWarning("Invalid JSON in data file, creating new structure");
                return new DataCollectionRoot();
            }
        }

        private string GetClientIpAddress()
        {
            // Try to get the real IP address from various headers
            var forwardedHeader = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            var realIpHeader = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIpHeader))
            {
                return realIpHeader;
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
