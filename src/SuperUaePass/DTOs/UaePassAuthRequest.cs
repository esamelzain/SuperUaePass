namespace SuperUaePass.DTOs;

/// <summary>
/// UAE Pass authentication request parameters
/// </summary>
public class UaePassAuthRequest
{
    /// <summary>
    /// Client ID for the UAE Pass application
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Response type for OAuth flow (usually "code")
    /// </summary>
    public string ResponseType { get; set; } = "code";

    /// <summary>
    /// Redirect URI after authentication
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Scope for the requested permissions
    /// </summary>
    public string Scope { get; set; } = "urn:uae:digitalid:profile:general";

    /// <summary>
    /// State parameter for CSRF protection
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Nonce for replay attack protection
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Prompt parameter (optional)
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// UI locales for internationalization
    /// </summary>
    public string? UiLocales { get; set; }
}
