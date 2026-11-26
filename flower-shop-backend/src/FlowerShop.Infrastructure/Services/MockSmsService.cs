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
        logger.LogInformation("[MOCK SMS] Phone: {Phone}, Code: {Code}", phone, code);
        return Task.CompletedTask;
    }
}

public class EskizSmsService(ILogger<EskizSmsService> logger) : ISmsService
{
    public Task SendOtpAsync(string phone, string code, CancellationToken ct = default)
    {
        // TODO: integrate with Eskiz.uz SMS API
        logger.LogInformation("[ESKIZ SMS] Sending to {Phone}", phone);
        return Task.CompletedTask;
    }
}
