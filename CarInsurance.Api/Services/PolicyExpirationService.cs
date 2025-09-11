using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class PolicyExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PolicyExpirationService> _logger;

    public PolicyExpirationService(IServiceScopeFactory scopeFactory, ILogger<PolicyExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndLogExpirations(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking policy expirations.");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    private async Task CheckAndLogExpirations(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dateTimeNow = DateTime.UtcNow;
        var dateOnlyYesterday = DateOnly.FromDateTime(dateTimeNow.AddDays(-1));

        var expiredPolicies = await db.Policies
            .Where(p => p.EndDate == dateOnlyYesterday && p.LoggedAt == DateTime.MinValue)
            .ToListAsync(stoppingToken);

        foreach (var policy in expiredPolicies)
        {
            policy.LoggedAt = dateTimeNow;
            string message = $"Policy {policy.Id} for car {policy.CarId} expired at {policy.EndDate}";
            _logger.LogInformation(message);
        }


        if (expiredPolicies.Any())
        {
            await db.SaveChangesAsync(stoppingToken);
        }
    }
}

