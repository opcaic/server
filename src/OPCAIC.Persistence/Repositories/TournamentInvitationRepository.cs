using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
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

		public async Task<ListDto<TournamentInvitationDto>> GetInvitationsAsync(long tournamentId,
			TournamentInvitationFilterDto filter, CancellationToken cancellationToken)
		{
			var query = DbSet.Where(row => row.TournamentId == tournamentId);

			if (filter != null)
			{
				query = query.Filter(filter);
			}

			return new ListDto<TournamentInvitationDto>
			{
				List = await query
					.Select(row => new TournamentInvitationDto
					{
						Id = row.Id,
						Email = row.Email,
						User = Context.Users
							.Where(usr => usr.Email == row.Email)
							.Select(usr
								=> new UserReferenceDto {Id = usr.Id, Username = usr.UserName})
							.FirstOrDefault()
					})
					.ToArrayAsync(cancellationToken),
				Total = await query.CountAsync(cancellationToken)
			};
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

			// TODO:
//			foreach (var email in emails)
//			{
//				tournament.Participants.Add(new TournamentInvitation {Email = email});
//			}

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