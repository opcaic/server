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
			builder.OwnsMany(e => e.MenuItems, b =>
			{
				b.Property<long>("Id");
				b.HasKey("Id");
			});
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
		}
	}
}