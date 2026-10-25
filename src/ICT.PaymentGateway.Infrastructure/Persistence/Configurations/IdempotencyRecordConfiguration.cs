using ICT.PaymentGateway.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICT.PaymentGateway.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Key).HasMaxLength(255).IsRequired();
        builder.Property(r => r.RequestHash).HasMaxLength(64).IsRequired();

        // Keys are scoped per merchant. The unique index also stops two concurrent requests both winning.
        builder.HasIndex(r => new { r.MerchantId, r.Key }).IsUnique();
    }
}
