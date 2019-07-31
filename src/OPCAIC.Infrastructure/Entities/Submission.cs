using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Represents a bot submission to a tournament.
	/// </summary>
	public class Submission : SoftDeletableEntity
	{
		/// <summary>
		///     Id of the user who created this submission.
		/// </summary>
		public long AuthorId { get; set; }

		/// <summary>
		///     Author of this submission.
		/// </summary>
		public virtual User Author { get; set; }

		/// <summary>
		///     Whether this submission is active and should participate in tournament matches.
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		///     Id of the tournament this submission has been posted to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     Tournament this submission has been posted to.
		/// </summary>
		public virtual Tournament Tournament { get; set; }

		/// <summary>
		///     Reference to mapping table of matches and their participants.
		/// </summary>
		public virtual IList<SubmissionParticipation> Participations { get; set; }

		/// <summary>
		///     All matches this submission participates in.
		/// </summary>
		[NotMapped]
		public IEnumerable<Match> Matches => Participations.Select(p => p.Match);

		/// <summary>
		///     All validations of this submission.
		/// </summary>
		public virtual IList<SubmissionValidation> Validations { get; set; }

		/// <summary>
		///     Last validation, result of which determines whether the submission is valid or not.
		/// </summary>
		[NotMapped]
		public SubmissionValidation LastValidation 
			=> Validations?.AsQueryable().OrderByDescending(v => v.Created).FirstOrDefault();

		/// <summary>
		///     Flag whether the submission has been successfully validated.
		/// </summary>
		[NotMapped]
		public bool IsValid => LastValidation?.ValidatorResult == GameModuleEntryPointResult.Success;
	}
}