using Microsoft.Extensions.Logging;

namespace FlowerShop.Infrastructure.Services;

public interface ISmsService
{
    Task SendOtpAsync(string phone, string code, CancellationToken ct = default);
}

public class MockSmsService(ILogger<MockSmsService> logger) : ISmsService
{
    public Task SendOtpAsync(string phone, string code, CancellationToken ct = default)
    {
        logger.LogInformation("[MOCK SMS] OTP delivery simulated");
        return Task.CompletedTask;
    }
}

public class EskizSmsService : ISmsService
{
    public Task SendOtpAsync(string phone, string code, CancellationToken ct = default)
    {
        throw new NotSupportedException("Eskiz SMS delivery is not implemented. Configure a working SMS provider before enabling real OTP login.");
    }
}
