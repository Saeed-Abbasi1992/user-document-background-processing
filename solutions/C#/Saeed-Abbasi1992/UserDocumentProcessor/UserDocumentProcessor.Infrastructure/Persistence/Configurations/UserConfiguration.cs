using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserDocumentProcessor.Domain.Entities;

namespace UserDocumentProcessor.Infrastructure.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);

        builder.HasMany(u => u.Documents)
               .WithOne()
               .HasForeignKey(d => d.UserId);
    }
}

