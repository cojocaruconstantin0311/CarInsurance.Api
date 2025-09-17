using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Tasks;

public class PolicyExpirationMonitor(IServiceProvider sp, ILogger<PolicyExpirationMonitor> logger) : BackgroundService
{
    private readonly IServiceProvider _sp = sp;
    private readonly ILogger<PolicyExpirationMonitor> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpirations(stoppingToken);
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (TaskCanceledException) { }
        }
    }

    internal async Task CheckExpirations(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var nowLocal = DateTime.Now;
        var oneHourAgoLocal = nowLocal.AddHours(-1);

        var policies = await db.Policies.AsNoTracking().ToListAsync(ct);

        foreach (var p in policies)
        {
            var expirationLocal = p.EndDate.ToDateTime(new TimeOnly(23, 59, 59));
            var hourAgo = DateTime.Now.AddHours(-1);

            _logger.LogInformation(
                "Policy {Id} Car {CarId}: EndDate={EndDate}, ExpirationLocal={Exp}, Window=({HourAgo} .. {Now})",
                p.Id, p.CarId, p.EndDate, expirationLocal, hourAgo, DateTime.Now);

            if (expirationLocal <= DateTime.Now && expirationLocal > hourAgo)
            {
                var already = await db.PolicyExpirationLogs.AnyAsync(x => x.PolicyId == p.Id, ct);
                if (!already)
                {
                    _logger.LogInformation(" -> Logging expiration for policy {Id}", p.Id);
                    db.PolicyExpirationLogs.Add(new PolicyExpirationLog
                    {
                        PolicyId = p.Id,
                        LoggedAtUtc = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync(ct);
                }
            }
        }
    }
}
