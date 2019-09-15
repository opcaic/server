using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
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