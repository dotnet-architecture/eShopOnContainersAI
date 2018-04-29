using eShopDashboard.EntityModels.Ordering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShopDashboard.Infrastructure.Data.Ordering
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders", schema: "Ordering");

            builder.Property(o => o.Id)
                .IsRequired(true)
                .ValueGeneratedNever();

            builder.Property(o => o.OrderDate)
                .IsRequired(true);

            builder.Property(o => o.Address_Country)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}