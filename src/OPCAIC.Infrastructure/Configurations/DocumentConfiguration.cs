using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Configurations
{
	public class DocumentConfiguration : IEntityTypeConfiguration<Document>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Document> builder)
		{
			builder.Property(e => e.Name).IsRequired().HasMaxLength(StringLengths.DocumentName);
		}
	}
}