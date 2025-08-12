# SuperUaePass

A comprehensive .NET library for integrating UAE Pass authentication into your applications.

**Based on official UAE Pass documentation**: [https://docs.uaepass.ae/feature-guides/authentication/web-application](https://docs.uaepass.ae/feature-guides/authentication/web-application)

## Features

- 🔐 **OAuth 2.0 Integration** - Complete UAE Pass OAuth 2.0 flow support
- 🚀 **Multi-target Support** - Supports .NET 6.0, 7.0, 8.0, and 9.0
- 🔧 **Easy Configuration** - Simple setup with dependency injection
- 🌐 **Proxy Support** - Built-in proxy configuration for enterprise environments
- 📝 **Comprehensive DTOs** - Full request/response models for UAE Pass API
- 🛡️ **Security First** - Proper token validation and CSRF protection
- 📊 **Logging Support** - Detailed logging for debugging and monitoring
- 🏛️ **Official Compliance** - Based on official UAE Pass documentation

## Installation

```bash
dotnet add package SuperUaePass
```

## Quick Start

### 1. Configure Services

```csharp
// In Program.cs or Startup.cs
builder.Services.AddSuperUaePass(options =>
{
    // UAE Pass Environment URLs
    options.BaseUrl = "https://stg-id.uaepass.ae"; // Staging environment
    // options.BaseUrl = "https://id.uaepass.ae"; // Production environment
    
    // Your UAE Pass credentials (provided during onboarding)
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret";
    options.RedirectUri = "https://your-app.com/callback";
    options.Scope = "urn:uae:digitalid:profile:general";
    
    // Optional: Configure proxy for enterprise environments
    options.UseProxy = true;
    options.ProxyUrl = "http://your-proxy:8080";
    options.ProxyUsername = "proxy-user";
    options.ProxyPassword = "proxy-password";
});
```

### 2. Use in Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISuperUaePassService _uaePassService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISuperUaePassService uaePassService, ILogger<AuthController> logger)
    {
        _uaePassService = uaePassService;
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var state = Guid.NewGuid().ToString();
        
        // Store state in session for validation
        HttpContext.Session.SetString("UaePassState", state);
        
        // Generate UAE Pass authorization URL for browser-based authentication
        // Note: UAE Pass staging doesn't require nonce parameter
        var authUrl = _uaePassService.GenerateAuthorizationUrl(state);
        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string? code = "", string? state = "", string? error = null, string? error_description = null)
    {
        var correlationId = Guid.NewGuid();
        
        try
        {
            // Check for authorization errors from UAE Pass
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest($"UAE Pass authorization failed: {error} - {error_description}");
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return BadRequest("Missing required parameters: code or state");
            }

            // Validate state parameter to prevent CSRF attacks
            var storedState = HttpContext.Session.GetString("UaePassState");
            if (string.IsNullOrEmpty(storedState) || state != storedState)
            {
                return BadRequest("Invalid state parameter - possible CSRF attack");
            }

            // Clear the state from session after validation
            HttpContext.Session.Remove("UaePassState");

            // Step 1: Exchange authorization code for access token
            var tokenResponse = await _uaePassService.ExchangeCodeForTokenAsync(code, state);
            
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return BadRequest("Failed to obtain access token from UAE Pass");
            }

            // Step 2: Get user profile using the access token
            var userProfile = await _uaePassService.GetUserProfileAsync(tokenResponse.AccessToken);
            
            if (userProfile == null)
            {
                return BadRequest("Failed to retrieve user profile from UAE Pass");
            }

            // Step 3: Validate user type
            if (!SuperUaePassService.IsUserTypeSupported(userProfile.UserType))
            {
                return BadRequest($"User type '{userProfile.UserType}' is not supported");
            }

            // Step 4: Validate Emirates ID (required for authentication)
            if (string.IsNullOrEmpty(userProfile.EmiratesId))
            {
                return BadRequest("User Emirates ID is not verified");
            }

            // Step 5: Store user information in session
            var sessionData = new
            {
                EmiratesId = userProfile.EmiratesId,
                Uuid = userProfile.Uuid,
                UserType = userProfile.UserType,
                FullNameEn = userProfile.FullNameEn,
                FullNameAr = userProfile.FullNameAr,
                Email = userProfile.Email,
                Mobile = userProfile.Mobile,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenExpiresIn = tokenResponse.ExpiresIn,
                TokenReceivedAt = DateTime.UtcNow
            };

            HttpContext.Session.SetString("UaePassUser", JsonSerializer.Serialize(sessionData));
            HttpContext.Session.SetString("UaePassAccessToken", tokenResponse.AccessToken);
            
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                HttpContext.Session.SetString("UaePassRefreshToken", tokenResponse.RefreshToken);
            }
            
            // Redirect to dashboard or return user data
            return RedirectToAction("Dashboard", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UAE Pass authentication failed. CorrelationId: {CorrelationId}", correlationId);
            return BadRequest($"UAE Pass authentication failed: {ex.Message}");
        }
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        // Clear session data
        HttpContext.Session.Clear();
        
        // Generate UAE Pass logout URL with redirect to your application
        var logoutUrl = _uaePassService.GenerateLogoutUrl("https://your-app.com/logged-out");
        return Redirect(logoutUrl);
    }

    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var userJson = HttpContext.Session.GetString("UaePassUser");
        if (string.IsNullOrEmpty(userJson))
        {
            return RedirectToAction("Login");
        }

        var userProfile = JsonSerializer.Deserialize<dynamic>(userJson);
        return Ok(userProfile);
    }

    [HttpGet("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = HttpContext.Session.GetString("UaePassRefreshToken");
        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest("No refresh token available");
        }

        try
        {
            var tokenResponse = await _uaePassService.RefreshTokenAsync(refreshToken);
            
            // Update session with new tokens
            HttpContext.Session.SetString("UaePassAccessToken", tokenResponse.AccessToken);
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                HttpContext.Session.SetString("UaePassRefreshToken", tokenResponse.RefreshToken);
            }

            return Ok(new { message = "Token refreshed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest($"Token refresh failed: {ex.Message}");
        }
    }
}
```

## Configuration Options

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `BaseUrl` | string | Yes | UAE Pass API base URL |
| `ClientId` | string | Yes | Your UAE Pass application client ID (from onboarding) |
| `ClientSecret` | string | Yes | Your UAE Pass application client secret (from onboarding) |
| `RedirectUri` | string | Yes | Redirect URI after authentication (must be registered) |
| `Scope` | string | No | OAuth scope (default: "urn:uae:digitalid:profile:general") |
| `ResponseType` | string | No | Response type (default: "code") |
| `Environment` | UaePassEnvironment | No | Environment type (Staging/Production) |
| `UseProxy` | bool | No | Whether to use proxy (default: false) |
| `ProxyUrl` | string | No | Proxy URL when UseProxy is true |
| `ProxyUsername` | string | No | Proxy username |
| `ProxyPassword` | string | No | Proxy password |
| `TimeoutSeconds` | int | No | HTTP timeout in seconds (default: 30) |
| `EnableLogging` | bool | No | Enable detailed logging (default: false) |

## UAE Pass Environments

### Staging Environment
- **URL**: `https://stg-id.uaepass.ae`
- **Purpose**: Testing and development
- **Credentials**: Provided during onboarding process

### Production Environment
- **URL**: `https://id.uaepass.ae`
- **Purpose**: Live applications
- **Credentials**: Provided after successful assessment

## Configuration from appsettings.json

```json
{
  "SuperUaePass": {
    "BaseUrl": "https://stg-id.uaepass.ae",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "https://your-app.com/callback",
    "Scope": "urn:uae:digitalid:profile:general",
    "Environment": "Staging",
    "UseProxy": false,
    "TimeoutSeconds": 30,
    "EnableLogging": true
  }
}
```

```csharp
// Register with configuration
builder.Services.AddSuperUaePass(builder.Configuration);
```

## API Reference

### ISuperUaePassService

#### GenerateAuthorizationUrl
Generates the authorization URL for UAE Pass authentication.

```csharp
string GenerateAuthorizationUrl(string state, string? nonce = null, string? prompt = null, string? uiLocales = null)
```

#### ExchangeCodeForTokenAsync
Exchanges authorization code for access token.

```csharp
Task<UaePassTokenResponse> ExchangeCodeForTokenAsync(string code, string state, CancellationToken cancellationToken = default)
```

#### GetUserProfileAsync
Retrieves user profile information using access token.

```csharp
Task<UaePassUserProfile> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default)
```

#### RefreshTokenAsync
Refreshes access token using refresh token.

```csharp
Task<UaePassTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
```

#### ValidateIdTokenAsync
Validates ID token.

```csharp
Task<bool> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
```

#### GenerateLogoutUrl
Generates logout URL for UAE Pass.

```csharp
string GenerateLogoutUrl(string? redirectUri = null)
```

## DTOs

### UaePassAuthRequest
Authentication request parameters.

### UaePassTokenResponse
Token response containing access token and related information.

**UAE Pass Token Response Format:**
```json
{
  "access_token": "67f2536e-07e6-37c1-967f-78562000a4f9",
  "scope": "urn:uae:digitalid:profile:general",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

**Key Properties:**
- `access_token` - Access token for API calls
- `token_type` - Token type (always "Bearer")
- `expires_in` - Expiration time in seconds
- `scope` - Scope of granted permissions

### UaePassUserProfile
User profile information including Emirates ID, name, email, etc.

**UAE Pass User Information Response Format:**
```json
{
    "sub": "UAEPASS/7a05992e-3244-49d3-bcbc-7894c8fca25e",
    "fullnameAR": "ساوميا,,,,شارما,,",
    "gender": "Male",
    "mobile": "97151234003",
    "lastnameEN": "ABC",
    "fullnameEN": "Ram,,,,ABC,,",
    "uuid": "7a05992e-3244-49d3-bcbc-7894c8fca25e",
    "lastnameAR": "شارما",
    "idn": "784189014978983",
    "nationalityEN": "IND",
    "firstnameEN": "Ram",
    "userType": "SOP3",
    "nationalityAR": "هندى",
    "firstnameAR": "ساوميا",
    "email": "user@example.com"
}
```

**Key Properties:**
- `sub` - Subject identifier (UAE Pass user identifier)
- `uuid` - User UUID
- `userType` - User type (e.g., "SOP3")
- `idn` - Emirates ID number (Identity Number)
- `firstnameEN/lastnameEN/fullnameEN` - Names in English
- `firstnameAR/lastnameAR/fullnameAR` - Names in Arabic
- `email` - Email address
- `mobile` - Mobile phone number
- `gender` - Gender
- `nationalityEN/nationalityAR` - Nationality in English/Arabic

## UAE Pass Integration Flow

1. **Authorization Request** → User redirected to UAE Pass (`/idshub/authorize`)
2. **Authorization Code** → UAE Pass returns code to your callback
3. **Token Exchange** → Exchange code for access token (`/idshub/token`)
4. **User Information** → Retrieve user profile with access token (`/idshub/userinfo`)
5. **Logout** → Logout user from UAE Pass (`/idshub/logout`)

### Browser-Based Authentication Flow

For browser-based authentication, the authorization URL includes the `acr_values` parameter for authentication level:

```
https://stg-id.uaepass.ae/idshub/authorize?response_type=code&client_id=YOUR_CLIENT_ID&scope=urn:uae:digitalid:profile:general&state=STATE&redirect_uri=YOUR_REDIRECT_URI&acr_values=urn:safelayer:tws:policies:authentication:level:low
```

**Important Note**: UAE Pass expects query parameters **without URL encoding**. The package automatically handles this for you.

**Note**: The `nonce` parameter is optional for UAE Pass staging environment. It's included for production environments that require additional security.

The callback will return to your controller with the authorization code, which you can then exchange for an access token.

## Security Considerations

1. **State Parameter**: Always validate the state parameter to prevent CSRF attacks
2. **Nonce**: Use nonce for replay attack protection
3. **HTTPS**: Always use HTTPS in production
4. **Token Storage**: Store tokens securely and never expose them in client-side code
5. **Token Validation**: Validate ID tokens before trusting user information
6. **UAE Pass Compliance**: Follow UAE Pass security guidelines

## User Type Validation

UAE Pass returns different user types that you may want to validate before allowing access:

```csharp
// Check if user type is supported
if (!SuperUaePassService.IsUserTypeSupported(userProfile.UserType))
{
    return BadRequest($"User type '{userProfile.UserType}' is not supported");
}

// Or check against custom list of supported types
var supportedTypes = new[] { "SOP3", "SOP2", "SOP1" };
if (!SuperUaePassService.IsUserTypeSupported(userProfile.UserType, supportedTypes))
{
    return BadRequest($"User type '{userProfile.UserType}' is not supported");
}
```

### Common UAE Pass User Types:
- **SOP3**: Standard user type (most common)
- **SOP2**: Another common user type
- **SOP1**: Basic user type

## Error Handling

The package provides comprehensive error handling for common UAE Pass scenarios:

```csharp
try
{
    var tokenResponse = await _uaePassService.ExchangeCodeForTokenAsync(code, state);
    var userProfile = await _uaePassService.GetUserProfileAsync(tokenResponse.AccessToken);
    
    // Validate user type and Emirates ID
    if (!SuperUaePassService.IsUserTypeSupported(userProfile.UserType))
    {
        return BadRequest($"User type '{userProfile.UserType}' is not supported");
    }
    
    if (string.IsNullOrEmpty(userProfile.EmiratesId))
    {
        return BadRequest("User Emirates ID is not verified");
    }
}
catch (HttpRequestException ex)
{
    // Handle UAE Pass API errors
    return BadRequest($"UAE Pass API error: {ex.Message}");
}
catch (Exception ex)
{
    // Handle other errors
    return BadRequest($"Authentication failed: {ex.Message}");
}
```

## Getting Started with UAE Pass

1. **Onboarding**: Complete UAE Pass onboarding process
2. **Credentials**: Receive client ID and secret
3. **Registration**: Register your redirect URIs
4. **Testing**: Test in staging environment
5. **Assessment**: Complete UAE Pass assessment
6. **Production**: Go live with production credentials

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, please open an issue on GitHub or contact the maintainers.

## UAE Pass Resources

- [Official UAE Pass Documentation](https://docs.uaepass.ae/)
- [UAE Pass Onboarding Guide](https://docs.uaepass.ae/getting-onboarded-with-uaepass)
- [UAE Pass Staging Environment](https://docs.uaepass.ae/quick-start-guide-uaepass-staging-environment)
