using SuperUaePass.DTOs;

namespace SuperUaePass.Services;

/// <summary>
/// Service interface for UAE Pass integration
/// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application
/// </summary>
public interface ISuperUaePassService
{
    /// <summary>
    /// Generates the authorization URL for UAE Pass authentication
    /// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application/1.-obtaining-the-oauth2-access-code
    /// </summary>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <param name="nonce">Nonce parameter for replay attack protection (optional)</param>
    /// <param name="prompt">Prompt parameter (optional)</param>
    /// <param name="uiLocales">UI locales parameter (optional)</param>
    /// <returns>Authorization URL</returns>
    string GenerateAuthorizationUrl(string state, string? nonce = null, string? prompt = null, string? uiLocales = null);

    /// <summary>
    /// Exchanges authorization code for access token
    /// </summary>
    /// <param name="code">Authorization code received from UAE Pass</param>
    /// <param name="state">State parameter for verification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token response containing access token</returns>
    Task<UaePassTokenResponse> ExchangeCodeForTokenAsync(string code, string state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves user profile information using access token
    /// </summary>
    /// <param name="accessToken">Access token for API calls</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile information</returns>
    Task<UaePassUserProfile> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New token response</returns>
    Task<UaePassTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates ID token
    /// </summary>
    /// <param name="idToken">ID token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is valid, false otherwise</returns>
    Task<bool> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates logout URL for UAE Pass
    /// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application/4.-web-single-sign-on-sso-and-logout-user-session-from-uae-pass
    /// </summary>
    /// <param name="redirectUri">Redirect URI after logout</param>
    /// <returns>Logout URL</returns>
    string GenerateLogoutUrl(string? redirectUri = null);
}
