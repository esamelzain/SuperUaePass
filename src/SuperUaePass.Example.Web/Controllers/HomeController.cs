using Microsoft.AspNetCore.Mvc;
using SuperUaePass.Services;
using SuperUaePass.DTOs;
using System.Text.Json;

namespace SuperUaePass.Example.Web.Controllers;

// Simplified model for session data - only Emirates ID
public class UaePassSessionData
{
    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public DateTime LoggedInAt { get; set; }
}

public class HomeController : Controller
{
    private readonly ISuperUaePassService _uaePassService;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ISuperUaePassService uaePassService, ILogger<HomeController> logger, IConfiguration configuration)
    {
        _uaePassService = uaePassService;
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
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
        _logger.LogInformation("Redirecting user to UAE Pass authorization: {Url}", authUrl);
        
        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string? code = "", string? state = "", string? error = null, string? error_description = null)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("UAE Pass callback received. CorrelationId: {CorrelationId}, Code: {Code}, State: {State}", 
            correlationId, !string.IsNullOrEmpty(code), !string.IsNullOrEmpty(state));

        try
        {
            // Check for authorization errors from UAE Pass
            if (!string.IsNullOrEmpty(error))
            {
                var errorMessage = $"UAE Pass authorization failed: {error} - {error_description}";
                _logger.LogError("UAE Pass authorization error. CorrelationId: {CorrelationId}, Error: {Error}", correlationId, errorMessage);
                ViewBag.Error = errorMessage;
                return View("Error");
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                var errorMessage = "Missing required parameters: code or state";
                _logger.LogError("Missing parameters. CorrelationId: {CorrelationId}, Code: {Code}, State: {State}", 
                    correlationId, !string.IsNullOrEmpty(code), !string.IsNullOrEmpty(state));
                ViewBag.Error = errorMessage;
                return View("Error");
            }

            // Validate state parameter to prevent CSRF attacks
            var storedState = HttpContext.Session.GetString("UaePassState");
            if (string.IsNullOrEmpty(storedState) || state != storedState)
            {
                var errorMessage = "Invalid state parameter - possible CSRF attack";
                _logger.LogError("State validation failed. CorrelationId: {CorrelationId}, StoredState: {StoredState}, ReceivedState: {ReceivedState}", 
                    correlationId, storedState, state);
                ViewBag.Error = errorMessage;
                return View("Error");
            }

            // Clear the state from session after validation
            HttpContext.Session.Remove("UaePassState");

            // Step 1: Exchange authorization code for access token
            _logger.LogInformation("Exchanging authorization code for token. CorrelationId: {CorrelationId}", correlationId);
            var tokenResponse = await _uaePassService.ExchangeCodeForTokenAsync(code, state);
            
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                var errorMessage = "Failed to obtain access token from UAE Pass";
                _logger.LogError("Token exchange failed. CorrelationId: {CorrelationId}", correlationId);
                ViewBag.Error = errorMessage;
                return View("Error");
            }

            // Step 2: Get user profile using the access token
            _logger.LogInformation("Retrieving user profile. CorrelationId: {CorrelationId}", correlationId);
            var userProfile = await _uaePassService.GetUserProfileAsync(tokenResponse.AccessToken);
            
            if (userProfile == null)
            {
                var errorMessage = "Failed to retrieve user profile from UAE Pass";
                _logger.LogError("User profile retrieval failed. CorrelationId: {CorrelationId}", correlationId);
                ViewBag.Error = errorMessage;
                return View("Error");
            }

            // Step 3: Validate user type
            if (!SuperUaePassService.IsUserTypeSupported(userProfile.UserType))
            {
                var errorMessage = $"User type '{userProfile.UserType}' is not supported";
                _logger.LogError("Unsupported user type. CorrelationId: {CorrelationId}, UserType: {UserType}", 
                    correlationId, userProfile.UserType);
                ViewBag.Error = errorMessage;
                return View("Error");
            }

            // Step 4: Validate Emirates ID (required for authentication)
            if (string.IsNullOrEmpty(userProfile.EmiratesId))
            {
                var errorMessage = "User Emirates ID is not verified";
                _logger.LogError("Emirates ID not verified. CorrelationId: {CorrelationId}, EmiratesID: {EmiratesId}", 
                    correlationId, userProfile.EmiratesId);
                ViewBag.Error = errorMessage;
                return View("Error");
            }

            // Step 5: Create simple session data with only Emirates ID and basic info
            var sessionData = new UaePassSessionData
            {
                EmiratesId = userProfile.EmiratesId,
                FullNameEn = userProfile.FullNameEn,
                FullNameAr = userProfile.FullNameAr,
                Email = userProfile.Email,
                Mobile = userProfile.Mobile,
                LoggedInAt = DateTime.UtcNow
            };

            // Store user session data
            HttpContext.Session.SetString("UaePassUser", JsonSerializer.Serialize(sessionData));
            
            // Set authentication cookie (simple approach)
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("UserEmiratesId", userProfile.EmiratesId);

            _logger.LogInformation("UAE Pass authentication successful. CorrelationId: {CorrelationId}, EmiratesId: {EmiratesId}, UserType: {UserType}", 
                correlationId, userProfile.EmiratesId, userProfile.UserType);
            
            // Redirect to profile page
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UAE Pass authentication failed. CorrelationId: {CorrelationId}", correlationId);
            ViewBag.Error = $"UAE Pass authentication failed: {ex.Message}";
            return View("Error");
        }
    }

    [HttpGet("profile")]
    public IActionResult Profile()
    {
        // Check if user is authenticated
        var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
        if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true")
        {
            _logger.LogWarning("User not authenticated, redirecting to login");
            return RedirectToAction("Index");
        }

        var userJson = HttpContext.Session.GetString("UaePassUser");
        if (string.IsNullOrEmpty(userJson))
        {
            _logger.LogWarning("User session not found, redirecting to login");
            return RedirectToAction("Index");
        }

        try
        {
            var sessionData = JsonSerializer.Deserialize<UaePassSessionData>(userJson);
            if (sessionData != null)
            {
                ViewBag.EmiratesId = sessionData.EmiratesId;
                ViewBag.FullName = sessionData.FullNameEn;
                ViewBag.UserProfile = sessionData;
            }
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize user session data");
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        var userJson = HttpContext.Session.GetString("UaePassUser");
        if (!string.IsNullOrEmpty(userJson))
        {
            try
            {
                var sessionData = JsonSerializer.Deserialize<UaePassSessionData>(userJson);
                if (sessionData != null)
                {
                    _logger.LogInformation("User logout. EmiratesId: {EmiratesId}", sessionData.EmiratesId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log user logout information");
            }
        }

        // Clear all session data
        HttpContext.Session.Clear();
        
        // Get logout redirect URL from configuration
        var logoutRedirectUrl = _configuration["Application:LogoutRedirectUrl"] ?? "https://localhost:7259";
        
        // Generate UAE Pass logout URL with redirect to your application
        var logoutUrl = _uaePassService.GenerateLogoutUrl(logoutRedirectUrl);
        return Redirect(logoutUrl);
    }

    [HttpGet("docs")]
    public IActionResult Documentation()
    {
        return View();
    }

    [HttpGet("about")]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet("analytics")]
    public IActionResult Analytics()
    {
        return View();
    }
}
