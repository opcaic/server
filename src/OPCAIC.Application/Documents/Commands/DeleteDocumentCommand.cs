using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Documents.Commands
{
	public class DeleteDocumentCommand : IIdentifiedRequest, IRequest
	{
		public DeleteDocumentCommand(long id)
		{
			Id = id;
		}

		/// <inheritdoc />
		public long Id { get; }

		public class Handler : IRequestHandler<DeleteDocumentCommand>
		{
			private readonly IRepository<Document> repository;

			public Handler(IRepository<Document> repository)
			{
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
			{
				await repository.DeleteAsync(request.Id, cancellationToken);
				return Unit.Value;
			}
		}
	}
}