using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FlowerShop.Application.DTOs.Auth;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FlowerShop.Infrastructure.Services;

public class AuthService(AppDbContext db, ISmsService sms, IConfiguration config)
{
    private readonly JwtSettings _jwt = config.GetSection("Jwt").Get<JwtSettings>()!;

    public async Task<SendOtpResponse> SendOtpAsync(string phone, CancellationToken ct)
    {
        var hourAgo = DateTimeOffset.UtcNow.AddHours(-1);
        var sendCount = await db.OtpCodes.CountAsync(o => o.Phone == phone && o.CreatedAt >= hourAgo, ct);
        if (sendCount >= 5)
            throw new AppException("RATE_LIMIT", "Превышен лимит отправок OTP");

        var isMock = config["Sms:Provider"]?.Equals("mock", StringComparison.OrdinalIgnoreCase) == true;
        var code = isMock ? "123456" : Random.Shared.Next(100000, 999999).ToString();
        var otp = new OtpCode
        {
            Phone = phone,
            Code = code,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.OtpCodes.Add(otp);
        await db.SaveChangesAsync(ct);
        await sms.SendOtpAsync(phone, code, ct);

        var masked = $"{phone[..6]}***{phone[^4..]}";
        return new SendOtpResponse(600, masked);
    }

    public async Task<AuthResponse> VerifyOtpAsync(string phone, string code, string? deviceInfo, string? ip, CancellationToken ct)
    {
        var otp = await db.OtpCodes
            .Where(o => o.Phone == phone && !o.IsUsed && o.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException("OTP_EXPIRED", "OTP-код не найден или истёк");

        if (otp.Attempts >= 3)
            throw new AppException("OTP_MAX_ATTEMPTS", "Превышен лимит попыток");

        if (otp.Code != code)
        {
            otp.Attempts++;
            await db.SaveChangesAsync(ct);
            throw new AppException("OTP_INVALID", "Неверный OTP-код");
        }

        otp.IsUsed = true;
        await db.SaveChangesAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == phone, ct);
        if (user is null)
        {
            var newId = Guid.NewGuid();
            user = new User
            {
                Id = newId,
                Phone = phone,
                Role = UserRole.Customer,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.Users.Add(user);
            db.LoyaltyAccounts.Add(new LoyaltyAccount
            {
                Id = Guid.NewGuid(),
                UserId = newId,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }

        if (user.IsBlocked)
            throw new AppException("FORBIDDEN", "Аккаунт заблокирован");

        return await CreateAuthResponseAsync(user, deviceInfo, ip, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, string? deviceInfo, string? ip, CancellationToken ct)
    {
        var hash = HashToken(refreshToken);
        var token = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && !t.IsRevoked && t.ExpiresAt > DateTimeOffset.UtcNow, ct)
            ?? throw new AppException("UNAUTHORIZED", "Refresh token недействителен");

        token.IsRevoked = true;
        await db.SaveChangesAsync(ct);

        return await CreateAuthResponseAsync(token.User, deviceInfo, ip, ct);
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken, CancellationToken ct)
    {
        if (refreshToken is null) return;
        var hash = HashToken(refreshToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.TokenHash == hash, ct);
        if (token is not null) token.IsRevoked = true;
        await db.SaveChangesAsync(ct);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, string? deviceInfo, string? ip, CancellationToken ct)
    {
        var accessToken = GenerateJwt(user);
        var refreshRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshHash = HashToken(refreshRaw);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            DeviceInfo = deviceInfo,
            IpAddress = ip,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken,
            refreshRaw,
            _jwt.AccessTokenExpiryMinutes * 60,
            new UserBriefDto(user.Id, user.Name, user.Phone, user.AvatarUrl, user.Role.ToString().ToLower())
        );
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("phone", user.Phone),
            new Claim("name", user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }
}

public class JwtSettings
{
    public string Issuer { get; set; } = "FlowerShopAPI";
    public string Audience { get; set; } = "FlowerShopClients";
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 30;
}

public class AppException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
