using FlowerShop.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace FlowerShop.Tests;

public class SmsServiceTests
{
    [Fact]
    public async Task Mock_does_not_log_phone_or_one_time_code()
    {
        var logger = new CapturingLogger();
        await new MockSmsService(logger).SendOtpAsync("+992900000000", "654321");

        Assert.Single(logger.Messages);
        Assert.DoesNotContain("+992900000000", logger.Messages[0]);
        Assert.DoesNotContain("654321", logger.Messages[0]);
    }

    [Fact]
    public async Task Unimplemented_provider_fails_instead_of_reporting_delivery()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            new EskizSmsService().SendOtpAsync("+992900000000", "654321"));
    }

    private sealed class CapturingLogger : ILogger<MockSmsService>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));

        private sealed class NoopScope : IDisposable
        {
            public static readonly NoopScope Instance = new();
            public void Dispose() { }
        }
    }
}
