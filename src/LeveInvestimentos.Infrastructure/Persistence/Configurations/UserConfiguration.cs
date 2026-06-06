using LeveInvestimentos.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeveInvestimentos.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.BirthDate)
            .IsRequired();

        builder.Property(user => user.ProfilePhotoStorageKey)
            .HasColumnName("ProfilePhotoStorageKey")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(user => user.MustChangePassword)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.OwnsOne(user => user.Address, address =>
        {
            address.Property(value => value.Street)
                .HasColumnName("AddressStreet")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(value => value.Number)
                .HasColumnName("AddressNumber")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(value => value.City)
                .HasColumnName("AddressCity")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(value => value.State)
                .HasColumnName("AddressState")
                .HasMaxLength(2)
                .IsRequired();
        });

        builder.OwnsOne(user => user.LandlinePhone, phone =>
        {
            phone.Property(value => value.Value)
                .HasColumnName("LandlinePhone")
                .HasMaxLength(30)
                .IsRequired();

            phone.Property(value => value.Digits)
                .HasColumnName("LandlinePhoneDigits")
                .HasMaxLength(13)
                .IsRequired();
        });

        builder.OwnsOne(user => user.MobilePhone, phone =>
        {
            phone.Property(value => value.Value)
                .HasColumnName("MobilePhone")
                .HasMaxLength(30)
                .IsRequired();

            phone.Property(value => value.Digits)
                .HasColumnName("MobilePhoneDigits")
                .HasMaxLength(13)
                .IsRequired();
        });

        builder.HasOne(user => user.Manager)
            .WithMany(user => user.Subordinates)
            .HasForeignKey(user => user.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
