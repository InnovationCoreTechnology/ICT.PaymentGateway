using ICT.PaymentGateway.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICT.PaymentGateway.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Plan).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(s => new { s.MerchantId, s.ExpiresAt });
        builder.HasIndex(s => s.PaymentId).IsUnique();
    }
}
