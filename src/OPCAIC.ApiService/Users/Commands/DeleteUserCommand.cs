using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Users.Commands
{
	public class DeleteUserCommand : AuthenticatedRequest, IRequest
	{
		public DeleteUserCommand(long userId)
		{
			UserId = userId;
		}

		public long UserId { get;  }

		public class Handler : IRequestHandler<DeleteUserCommand>
		{
			private readonly IRepository<User> repository;
			private readonly IRepository<TournamentInvitation> invitationRepository;
			private readonly IRepository<TournamentManager> managerRepository;

			public Handler(IRepository<User> repository, IRepository<TournamentInvitation> invitationRepository, IRepository<TournamentManager> managerRepository)
			{
				this.repository = repository;
				this.invitationRepository = invitationRepository;
				this.managerRepository = managerRepository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
			{
				// get the user first
				var user = await repository.GetAsync(request.UserId, new []
				{
					nameof(User.OwnedTournaments),
				}, cancellationToken);

				var newOwner = (await repository.ListAsync(new BaseSpecification<User>()
					.WithPaging(0, 1)
					.AddCriteria(u => u.Role == UserRole.Admin && u.Id != request.UserId), cancellationToken)).SingleOrDefault();
				if (newOwner == null)
				{
					// trying to delete last admin user, prevent lockout
					throw new BusinessException("Cannot delete last admin user.");
				}

				foreach (var tournament in user.OwnedTournaments)
				{
					tournament.Owner = newOwner;
					tournament.OwnerId = newOwner.Id;
				}

				repository.Delete(user);
				await repository.SaveChangesAsync(cancellationToken);
				return Unit.Value;
			}
		}
	}
}