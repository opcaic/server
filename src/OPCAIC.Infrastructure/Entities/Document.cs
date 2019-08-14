﻿using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Represents a markdown document, used for description of a tournament.
	/// </summary>
	public class Document : SoftDeletableEntity
	{
		/// <summary>
		///     Name of the document.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.DocumentName)]
		public string Name { get; set; }

		/// <summary>
		///     Content of the document, written in markdown.
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		///     Id of the tournament this document belongs to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     The tournament this document belongs to.
		/// </summary>
		public virtual Tournament Tournament { get; set; }

		internal static void OnModelCreating(EntityTypeBuilder<Document> builder)
		{
			builder.HasIndex(d => d.Name).IsUnique();
		}
	}
}