using ICT.PaymentGateway.Application.Common;
using ICT.PaymentGateway.Application.Payments;
using ICT.PaymentGateway.Infrastructure.Persistence;
using ICT.PaymentGateway.Infrastructure.Psp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ICT.PaymentGateway.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "PaymentGateway";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<PaymentGatewayDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history")));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPspGateway, FakePspGateway>();

        return services;
    }
}
