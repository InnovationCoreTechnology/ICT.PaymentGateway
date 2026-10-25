using ICT.PaymentGateway.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICT.PaymentGateway.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("amount").HasPrecision(19, 4).IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).IsFixedLength().IsRequired();
        });
        builder.Navigation(p => p.Amount).IsRequired();

        builder.Property(p => p.PaymentMethodToken).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.PspReference).HasMaxLength(100);
        builder.Property(p => p.FailureReason).HasMaxLength(500);

        builder.HasIndex(p => new { p.MerchantId, p.CreatedAt });
    }
}
