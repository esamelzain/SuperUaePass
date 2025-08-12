using System.Text.Json.Serialization;

namespace SuperUaePass.DTOs;

/// <summary>
/// UAE Pass token response containing access token and related information
/// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application/2.-obtaining-the-access-token
/// </summary>
public class UaePassTokenResponse
{
    /// <summary>
    /// Access token for API calls
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer" for UAE Pass)
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Expiration time in seconds
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Scope of the granted permissions
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Refresh token for getting new access tokens (if supported)
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// ID token containing user information (if openid scope requested)
    /// </summary>
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }

    /// <summary>
    /// State parameter returned from the original request
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }
}
