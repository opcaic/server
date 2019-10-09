using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
{
	public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<EmailTemplate> builder)
		{
			builder.HasKey(p => new {p.Name, p.LanguageCode});
		}
	}
}