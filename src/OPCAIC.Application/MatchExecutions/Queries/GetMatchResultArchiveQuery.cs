using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;

namespace OPCAIC.Application.MatchExecutions.Queries
{
	public class GetMatchResultArchiveQuery : AuthenticatedRequest, IRequest<Stream>
	{
		public GetMatchResultArchiveQuery(long matchExecutionId)
		{
			MatchExecutionId = matchExecutionId;
		}

		public long MatchExecutionId { get; }

		public class Handler : IRequestHandler<GetMatchResultArchiveQuery, Stream>
		{
			private readonly IMatchExecutionRepository repository;
			private readonly IStorageService storage;

			public Handler(IMatchExecutionRepository repository, IStorageService storage)
			{
				this.repository = repository;
				this.storage = storage;
			}

			/// <inheritdoc />
			public async Task<Stream> Handle(GetMatchResultArchiveQuery request,
				CancellationToken cancellationToken)
			{
				var storageDto =
					await repository.FindExecutionForStorageAsync(request.MatchExecutionId,
						cancellationToken);

				return storage.ReadMatchResultArchive(storageDto);
			}
		}
	}
}