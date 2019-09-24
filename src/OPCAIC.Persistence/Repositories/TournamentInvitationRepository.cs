using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class TournamentInvitationRepository
		: EntityRepository<TournamentInvitation>, ITournamentInvitationRepository
	{
		public TournamentInvitationRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		public async Task<bool> CreateAsync(long tournamentId, IEnumerable<string> emails,
			CancellationToken cancellationToken)
		{
			var tournament =
				await Context.Tournaments.SingleOrDefaultAsync(row => row.Id == tournamentId,
					cancellationToken);
			if (tournament == null)
			{
				return false;
			}

			foreach (var email in emails)
			{
				tournament.Invitations.Add(new TournamentInvitation {Email = email, TournamentId = tournamentId});
			}

			await SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> DeleteAsync(long tounamentId, string email,
			CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(
				row => row.TournamentId == tounamentId && row.Email == email, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			DbSet.Remove(entity);
			await SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}