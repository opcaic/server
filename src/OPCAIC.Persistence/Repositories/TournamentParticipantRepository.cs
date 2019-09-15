﻿using System.Collections.Generic;
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
	public class TournamentParticipantRepository
		: RepositoryBase<TournamentParticipant>, ITournamentParticipantRepository
	{
		public TournamentParticipantRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		public async Task<ListDto<TournamentParticipantDto>> GetParticipantsAsync(long tournamentId,
			TournamentParticipantFilterDto filter, CancellationToken cancellationToken)
		{
			var query = DbSet.Where(row => row.TournamentId == tournamentId);

			if (filter != null)
			{
				query = query.Filter(filter);
			}

			return new ListDto<TournamentParticipantDto>
			{
				List = await query
					.Select(row => new TournamentParticipantDto
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

			foreach (var email in emails)
			{
				tournament.Participants.Add(new TournamentParticipant {Email = email});
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