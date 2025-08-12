namespace SuperUaePass.Configuration;

/// <summary>
/// Configuration options for UAE Pass integration
/// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application
/// </summary>
public class SuperUaePassOptions
{
    /// <summary>
    /// The base URL for UAE Pass API
    /// Staging: https://staging-id.uaepass.ae
    /// Production: https://id.uaepass.ae
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Client ID for UAE Pass application (provided during onboarding)
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client Secret for UAE Pass application (provided during onboarding)
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Redirect URI for UAE Pass authentication (must be registered during onboarding)
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Scope for UAE Pass authentication
    /// Default: urn:uae:digitalid:profile:general
    /// Additional scopes may be available based on UAE Pass documentation
    /// </summary>
    public string Scope { get; set; } = "urn:uae:digitalid:profile:general";

    /// <summary>
    /// Response type for OAuth flow (fixed to "code" for authorization code flow)
    /// </summary>
    public string ResponseType { get; set; } = "code";

    /// <summary>
    /// Whether to use proxy for HTTP requests (for enterprise environments)
    /// </summary>
    public bool UseProxy { get; set; } = false;

    /// <summary>
    /// Proxy URL when UseProxy is true
    /// </summary>
    public string? ProxyUrl { get; set; }

    /// <summary>
    /// Proxy username when UseProxy is true
    /// </summary>
    public string? ProxyUsername { get; set; }

    /// <summary>
    /// Proxy password when UseProxy is true
    /// </summary>
    public string? ProxyPassword { get; set; }

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable detailed logging
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// Environment type (Staging/Production)
    /// </summary>
    public UaePassEnvironment Environment { get; set; } = UaePassEnvironment.Staging;
}

/// <summary>
/// UAE Pass environment types
/// </summary>
public enum UaePassEnvironment
{
    /// <summary>
    /// Staging environment for testing
    /// </summary>
    Staging,

    /// <summary>
    /// Production environment
    /// </summary>
    Production
}
