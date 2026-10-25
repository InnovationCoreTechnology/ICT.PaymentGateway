using ICT.PaymentGateway.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace ICT.PaymentGateway.Infrastructure.Persistence;

public sealed class PaymentGatewayDbContext(DbContextOptions<PaymentGatewayDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentGatewayDbContext).Assembly);
}
