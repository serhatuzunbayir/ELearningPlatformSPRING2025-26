namespace learning_platform.Models;

public record RegisterDto(string Name, string Email, string Password, string? PreferredCategory = null);

public record LoginDto(string Email, string Password);

public record VerifyTwoFactorDto(string Email, string Code);

public record AuthResponseDto(string Token, string Name, string Email, string Role);

public record LoginStepResponseDto(
    bool RequiresTwoFactor,
    string? Token,
    string? Name,
    string? Email,
    string? Role,
    string? Message,
    string? DevVerificationCode);
