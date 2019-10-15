using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class TournamentAdditionalFilesUploadedCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<TournamentAdditionalFilesUploadedCommand>
		{
			private readonly IRepository<Tournament> repository;

			public Handler(IRepository<Tournament> repository)
			{
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(TournamentAdditionalFilesUploadedCommand request,
				CancellationToken cancellationToken)
			{
				var updateDto = new TournamentAdditionalFilesUploadedDto(true);

				await repository.UpdateAsync(request.TournamentId, updateDto, cancellationToken);
				return Unit.Value;
			}
		}
	}
}