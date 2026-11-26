namespace FlowerShop.Application.DTOs.Auth;

public record SendOtpRequest(string Phone);
public record SendOtpResponse(int ExpiresInSeconds, string MaskedPhone);

public record VerifyOtpRequest(string Phone, string Code);
public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserBriefDto User);

public record UserBriefDto(
    Guid Id,
    string Name,
    string Phone,
    string? AvatarUrl,
    string Role);
