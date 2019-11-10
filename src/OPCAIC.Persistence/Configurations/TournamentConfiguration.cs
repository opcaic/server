using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.Persistence.Configurations
{
	public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Tournament> builder)
		{
			builder.Property(e => e.Name).IsRequired().HasMaxLength(StringLengths.TournamentName);
			builder.HasMany(e => e.MenuItems).WithOne().OnDelete(DeleteBehavior.Cascade);
			builder.Property(e => e.Configuration).IsRequired();
			builder.HasMany(e => e.Submissions).WithOne(e => e.Tournament);
		}
	}

	public class MenuItemConfiguration
		: IEntityTypeConfiguration<MenuItem>,
			IEntityTypeConfiguration<DocumentLinkMenuItem>,
			IEntityTypeConfiguration<ExternalUrlMenuItem>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<DocumentLinkMenuItem> builder)
		{
			builder.HasBaseType<MenuItem>();
		}

		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<ExternalUrlMenuItem> builder)
		{
			builder.HasBaseType<MenuItem>();
			builder.Property(e => e.ExternalLink).HasMaxLength(StringLengths.Url);
			builder.Property(e => e.Text).HasMaxLength(StringLengths.MenuItemText);
		}

		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<MenuItem> builder)
		{
			builder.Property(typeof(long), nameof(Entity.Id));
			builder.HasDiscriminator(p => p.Type)
				.HasValue<DocumentLinkMenuItem>(MenuItemType.DocumentLink)
				.HasValue<ExternalUrlMenuItem>(MenuItemType.ExternalUrl);
		}
	}
}