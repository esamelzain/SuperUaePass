using System.Text.Json.Serialization;

namespace SuperUaePass.DTOs;

/// <summary>
/// UAE Pass user profile information
/// Based on official UAE Pass documentation: https://docs.uaepass.ae/feature-guides/authentication/web-application/3.-obtaining-authenticated-user-information-from-the-access-token
/// </summary>
public class UaePassUserProfile
{
    /// <summary>
    /// Subject identifier (UAE Pass user identifier)
    /// </summary>
    [JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;

    /// <summary>
    /// User UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// User type (e.g., "SOP3")
    /// </summary>
    [JsonPropertyName("userType")]
    public string UserType { get; set; } = string.Empty;

    /// <summary>
    /// Profile type (for visitors)
    /// </summary>
    [JsonPropertyName("profileType")]
    public string? ProfileType { get; set; }

    /// <summary>
    /// Emirates ID number (IDN - Identity Number)
    /// </summary>
    [JsonPropertyName("idn")]
    public string? Idn { get; set; }

    /// <summary>
    /// Unified ID (for visitors)
    /// </summary>
    [JsonPropertyName("unifiedID")]
    public string? UnifiedId { get; set; }

    /// <summary>
    /// ID type
    /// </summary>
    [JsonPropertyName("idType")]
    public string? IdType { get; set; }

    /// <summary>
    /// Service Provider UUID
    /// </summary>
    [JsonPropertyName("spuuid")]
    public string? SpUuid { get; set; }

    /// <summary>
    /// First name in English
    /// </summary>
    [JsonPropertyName("firstnameEN")]
    public string FirstNameEn { get; set; } = string.Empty;

    /// <summary>
    /// Last name in English
    /// </summary>
    [JsonPropertyName("lastnameEN")]
    public string LastNameEn { get; set; } = string.Empty;

    /// <summary>
    /// Full name in English
    /// </summary>
    [JsonPropertyName("fullnameEN")]
    public string FullNameEn { get; set; } = string.Empty;

    /// <summary>
    /// First name in Arabic
    /// </summary>
    [JsonPropertyName("firstnameAR")]
    public string? FirstNameAr { get; set; }

    /// <summary>
    /// Last name in Arabic
    /// </summary>
    [JsonPropertyName("lastnameAR")]
    public string? LastNameAr { get; set; }

    /// <summary>
    /// Full name in Arabic
    /// </summary>
    [JsonPropertyName("fullnameAR")]
    public string? FullNameAr { get; set; }

    /// <summary>
    /// Title in English
    /// </summary>
    [JsonPropertyName("titleEN")]
    public string? TitleEn { get; set; }

    /// <summary>
    /// Title in Arabic
    /// </summary>
    [JsonPropertyName("titleAR")]
    public string? TitleAr { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Mobile phone number
    /// </summary>
    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    /// <summary>
    /// Gender
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality in English
    /// </summary>
    [JsonPropertyName("nationalityEN")]
    public string? NationalityEn { get; set; }

    /// <summary>
    /// Nationality in Arabic
    /// </summary>
    [JsonPropertyName("nationalityAR")]
    public string? NationalityAr { get; set; }

    /// <summary>
    /// Authentication Context Class Reference
    /// </summary>
    [JsonPropertyName("acr")]
    public string? Acr { get; set; }

    /// <summary>
    /// Authentication Methods References
    /// </summary>
    [JsonPropertyName("amr")]
    public string[]? Amr { get; set; }

    /// <summary>
    /// Additional properties that might be returned by UAE Pass
    /// This allows for flexibility with future API changes
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties { get; set; }

    // Convenience properties for backward compatibility
    /// <summary>
    /// Emirates ID (alias for Idn)
    /// </summary>
    [JsonIgnore]
    public string EmiratesId => Idn ?? string.Empty;

    /// <summary>
    /// First name (alias for FirstNameEn)
    /// </summary>
    [JsonIgnore]
    public string FirstName => FirstNameEn;

    /// <summary>
    /// Last name (alias for LastNameEn)
    /// </summary>
    [JsonIgnore]
    public string LastName => LastNameEn;

    /// <summary>
    /// Full name (alias for FullNameEn)
    /// </summary>
    [JsonIgnore]
    public string FullName => FullNameEn;

    /// <summary>
    /// Phone number (alias for Mobile)
    /// </summary>
    [JsonIgnore]
    public string? PhoneNumber => Mobile;
}
