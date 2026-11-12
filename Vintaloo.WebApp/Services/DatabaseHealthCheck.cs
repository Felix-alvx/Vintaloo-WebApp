using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Vintaloo.WebApp.Services
{
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly VintalooDbContext _context;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(VintalooDbContext context, ILogger<DatabaseHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var disponible = await _context.Database.CanConnectAsync(cancellationToken);
                return disponible
                    ? HealthCheckResult.Healthy("Base de datos disponible.")
                    : HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falló el health check de base de datos.");
                return HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.", ex);
            }
        }
    }
}
