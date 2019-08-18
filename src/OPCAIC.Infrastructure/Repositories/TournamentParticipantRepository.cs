using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public class TournamentParticipantRepository: RepositoryBase<TournamentParticipant>, ITournamentParticipantRepository
	{
		public TournamentParticipantRepository(DataContext context, IMapper mapper)
			:base(context, mapper) { }

		public Task<TournamentParticipantDto[]> GetParticipantsAsync(long tournamentId, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.TournamentId == tournamentId)
				.Select(row => new TournamentParticipantDto
				{
					Id = row.Id,
					Email = row.Email,
					User = Context.Users
					.Where(usr => usr.Email == row.Email)
					.Select(usr => new UserReferenceDto { Id = usr.Id, Username = usr.UserName })
					.FirstOrDefault()
				})
				.ToArrayAsync(cancellationToken);
		}

		public async Task<bool> CreateAsync(long tournamentId, IEnumerable<string> emails, CancellationToken cancellationToken)
		{
			var tournament = await Context.Tournaments.SingleOrDefaultAsync(row => row.Id == tournamentId, cancellationToken);
			if (tournament == null)
				return false;

			foreach (string email in emails)
			{
				tournament.Participants.Add(new TournamentParticipant { Email = email });
			}						  

			await SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> DeleteAsync(long tounamentId, string email, CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.TournamentId == tounamentId && row.Email == email, cancellationToken);
			if (entity == null)
				return false;

			DbSet.Remove(entity);
			await SaveChangesAsync(cancellationToken);
			return true;
		}
	}																						  
}
