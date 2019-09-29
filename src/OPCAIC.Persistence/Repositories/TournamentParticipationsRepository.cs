using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class TournamentParticipationsRepository 
		: Repository<TournamentParticipation>,
			IRepository<TournamentParticipation>
	{
		/// <inheritdoc />
		public TournamentParticipationsRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}
	}
}