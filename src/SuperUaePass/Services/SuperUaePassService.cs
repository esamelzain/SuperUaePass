using System.Text;
using System.Text.Json;
using SuperUaePass.Configuration;
using SuperUaePass.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace SuperUaePass.Services;

/// <summary>
/// Implementation of UAE Pass service
/// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application
/// </summary>
public class SuperUaePassService : ISuperUaePassService
{
    private readonly HttpClient _httpClient;
    private readonly SuperUaePassOptions _options;
    private readonly ILogger<SuperUaePassService>? _logger;

    /// <summary>
    /// Initializes a new instance of the SuperUaePassService
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="options">Configuration options</param>
    /// <param name="logger">Optional logger</param>
    public SuperUaePassService(
        HttpClient httpClient,
        IOptions<SuperUaePassOptions> options,
        ILogger<SuperUaePassService>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <inheritdoc/>
    public string GenerateAuthorizationUrl(string state, string nonce, string? prompt = null, string? uiLocales = null)
    {
        if (string.IsNullOrEmpty(state))
            throw new ArgumentException("State parameter is required", nameof(state));

        // Build authorization URL according to UAE Pass documentation
        // Note: UAE Pass expects parameters without URL encoding
        var queryParams = new List<string>
        {
            $"response_type={_options.ResponseType}",
            $"client_id={_options.ClientId}",
            $"scope={_options.Scope}",
            $"state={state}",
            $"redirect_uri={_options.RedirectUri}",
            $"acr_values=urn:safelayer:tws:policies:authentication:level:low"
        };

        // Only add nonce if provided (UAE Pass staging doesn't require it)
        if (!string.IsNullOrEmpty(nonce))
            queryParams.Add($"nonce={nonce}");

        if (!string.IsNullOrEmpty(prompt))
            queryParams.Add($"prompt={prompt}");

        if (!string.IsNullOrEmpty(uiLocales))
            queryParams.Add($"ui_locales={uiLocales}");

        // UAE Pass authorization endpoint (correct endpoint for browser-based auth)
        var authorizationUrl = $"{_options.BaseUrl.TrimEnd('/')}/idshub/authorize?{string.Join("&", queryParams)}";
        
        _logger?.LogDebug("Generated UAE Pass authorization URL: {Url}", authorizationUrl);
        
        return authorizationUrl;
    }

    /// <inheritdoc/>
    public async Task<UaePassTokenResponse> ExchangeCodeForTokenAsync(string code, string state, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Authorization code is required", nameof(code));

        if (string.IsNullOrEmpty(state))
            throw new ArgumentException("State parameter is required", nameof(state));

        // UAE Pass token endpoint
        var tokenUrl = $"{_options.BaseUrl.TrimEnd('/')}/idshub/token";
        
        _logger?.LogDebug("Exchanging authorization code for token at UAE Pass endpoint: {Url}", tokenUrl);

        // Build URL with query parameters as per real-world implementation
        // Note: UAE Pass expects parameters without URL encoding
        var queryParams = new List<string>
        {
            $"grant_type=authorization_code",
            $"redirect_uri={_options.RedirectUri}",
            $"code={code}"
        };
        
        var fullTokenUrl = $"{tokenUrl}?{string.Join("&", queryParams)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, fullTokenUrl);

        // UAE Pass uses Basic authentication with client credentials
        var clientId = _options.ClientId;
        var clientSecret = _options.ClientSecret;
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        // Accept JSON response
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger?.LogError("UAE Pass token exchange failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException($"UAE Pass token exchange failed: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<UaePassTokenResponse>(responseContent);
        
        if (tokenResponse == null)
            throw new InvalidOperationException("Failed to deserialize UAE Pass token response");

        if (string.IsNullOrEmpty(tokenResponse.AccessToken))
            throw new InvalidOperationException("UAE Pass returned empty access token");

        _logger?.LogDebug("Successfully exchanged authorization code for UAE Pass access token");
        
        return tokenResponse;
    }

    /// <inheritdoc/>
    public async Task<UaePassUserProfile> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException("Access token is required", nameof(accessToken));

        // UAE Pass user information endpoint
        var profileUrl = $"{_options.BaseUrl.TrimEnd('/')}/idshub/userinfo";
        
        _logger?.LogDebug("Retrieving user profile from UAE Pass endpoint: {Url}", profileUrl);

        using var request = new HttpRequestMessage(HttpMethod.Get, profileUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger?.LogError("UAE Pass profile retrieval failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException($"UAE Pass profile retrieval failed: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var userProfile = JsonSerializer.Deserialize<UaePassUserProfile>(responseContent);
        
        if (userProfile == null)
            throw new InvalidOperationException("Failed to deserialize UAE Pass user profile");

        _logger?.LogDebug("Successfully retrieved UAE Pass user profile for Emirates ID: {EmiratesId}", userProfile.EmiratesId);
        
        return userProfile;
    }

    /// <inheritdoc/>
    public async Task<UaePassTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Refresh token is required", nameof(refreshToken));

        // UAE Pass token endpoint for refresh
        var tokenUrl = $"{_options.BaseUrl.TrimEnd('/')}/idshub/token";
        
        _logger?.LogDebug("Refreshing UAE Pass access token at endpoint: {Url}", tokenUrl);

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        
        // Add query parameters for refresh token flow
        // Note: UAE Pass expects parameters without URL encoding
        var queryParams = new List<string>
        {
            $"grant_type=refresh_token",
            $"refresh_token={refreshToken}"
        };
        
        request.RequestUri = new Uri($"{tokenUrl}?{string.Join("&", queryParams)}");

        // UAE Pass uses Basic authentication with client credentials
        var clientId = _options.ClientId;
        var clientSecret = _options.ClientSecret;
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        // UAE Pass requires multipart/form-data content type
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger?.LogError("UAE Pass token refresh failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException($"UAE Pass token refresh failed: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<UaePassTokenResponse>(responseContent);
        
        if (tokenResponse == null)
            throw new InvalidOperationException("Failed to deserialize UAE Pass token response");

        _logger?.LogDebug("Successfully refreshed UAE Pass access token");
        
        return tokenResponse;
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(idToken))
            throw new ArgumentException("ID token is required", nameof(idToken));

        // TODO: Implement UAE Pass ID token validation
        // This should validate the JWT token signature using UAE Pass public keys
        // and check the claims against UAE Pass requirements
        
        _logger?.LogDebug("Validating UAE Pass ID token");
        
        try
        {
            // TODO: Implement proper JWT validation for UAE Pass
            // 1. Decode the JWT header to get the key ID
            // 2. Fetch the public key from UAE Pass JWKS endpoint
            // 3. Validate the signature
            // 4. Check claims (iss, aud, exp, iat, nonce, etc.)
            // 5. Verify the token is from UAE Pass
            
            return true; // Placeholder return - needs actual UAE Pass JWT validation
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UAE Pass ID token validation failed");
            return false;
        }
    }

    /// <summary>
    /// Validates if the UAE Pass user type is supported
    /// </summary>
    /// <param name="userType">The user type from UAE Pass response</param>
    /// <param name="supportedTypes">List of supported user types (case-insensitive)</param>
    /// <returns>True if user type is supported, false otherwise</returns>
    public static bool IsUserTypeSupported(string userType, IEnumerable<string> supportedTypes)
    {
        if (string.IsNullOrEmpty(userType))
            return false;

        return supportedTypes.Any(supportedType => 
            string.Equals(userType, supportedType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates if the UAE Pass user type is supported using common UAE Pass user types
    /// </summary>
    /// <param name="userType">The user type from UAE Pass response</param>
    /// <returns>True if user type is supported, false otherwise</returns>
    public static bool IsUserTypeSupported(string userType)
    {
        // Common UAE Pass user types based on real-world implementations
        var supportedTypes = new[]
        {
            "SOP3",  // Standard user type
            "SOP2",  // Another common user type
            "SOP1"   // Basic user type
        };

        return IsUserTypeSupported(userType, supportedTypes);
    }

    /// <summary>
    /// Logs out the user from UAE Pass
    /// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application/4.-web-single-sign-on-sso-and-logout-user-session-from-uae-pass
    /// </summary>
    /// <param name="redirectUri">Redirect URI after logout</param>
    /// <returns>Logout URL</returns>
    public string GenerateLogoutUrl(string? redirectUri = null)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(redirectUri))
        {
            queryParams.Add($"redirect_uri={Uri.EscapeDataString(redirectUri)}");
        }

        // UAE Pass logout endpoint (correct endpoint from documentation)
        var logoutUrl = $"{_options.BaseUrl.TrimEnd('/')}/idshub/logout";
        if (queryParams.Any())
        {
            logoutUrl += $"?{string.Join("&", queryParams)}";
        }
        
        _logger?.LogDebug("Generated UAE Pass logout URL: {Url}", logoutUrl);
        
        return logoutUrl;
    }
}
