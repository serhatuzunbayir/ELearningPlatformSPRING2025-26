using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LearningPlatform.API.Data;
using LearningPlatform.API.DTOs;
using LearningPlatform.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LearningPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, IConfiguration config, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Bu e-posta adresi zaten kayıtlı.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Student,
            PreferredCategory = string.IsNullOrWhiteSpace(dto.PreferredCategory) ? null : dto.PreferredCategory.Trim()
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new { message = "Kayıt başarılı." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("E-posta veya şifre hatalı.");

        if (user.TwoFactorEnabled)
        {
            // 2FA code is valid for 5 minutes.
            var code = Random.Shared.Next(100000, 999999).ToString();
            user.TwoFactorCode = code;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            await db.SaveChangesAsync();

            Console.WriteLine($"[2FA] {user.Email} verification code: {code}");

            return Ok(new LoginStepResponseDto(
                RequiresTwoFactor: true,
                Token: null,
                Name: null,
                Email: user.Email,
                Role: null,
                Message: "Enter the verification code sent to your account.",
                DevVerificationCode: env.IsDevelopment() ? code : null));
        }

        var token = GenerateToken(user);
        return Ok(new LoginStepResponseDto(
            RequiresTwoFactor: false,
            Token: token,
            Name: user.Name,
            Email: user.Email,
            Role: user.Role.ToString(),
            Message: null,
            DevVerificationCode: null));
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
            return Unauthorized("Invalid verification request.");

        if (user.TwoFactorCode != dto.Code || user.TwoFactorCodeExpiry is null || user.TwoFactorCodeExpiry < DateTime.UtcNow)
            return Unauthorized("Invalid or expired verification code.");

        user.TwoFactorCode = null;
        user.TwoFactorCodeExpiry = null;
        await db.SaveChangesAsync();

        var token = GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.Name, user.Email, user.Role.ToString()));
    }

    private string GenerateToken(User user)
    {
        // Role claim is used by [Authorize(Roles = "...")].
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
