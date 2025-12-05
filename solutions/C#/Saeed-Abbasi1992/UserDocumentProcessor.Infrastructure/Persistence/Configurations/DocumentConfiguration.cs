using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserDocumentProcessor.Domain.Entities;

namespace UserDocumentProcessor.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<DocumentEntity>
{
    public void Configure(EntityTypeBuilder<DocumentEntity> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Path)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(d => d.ProcessResultPath)
               .HasMaxLength(500);

        builder.Property(d => d.FileName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(d => d.ProcessStatus)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(d => d.IsDeleted)
               .HasDefaultValue(false);
    }
}
