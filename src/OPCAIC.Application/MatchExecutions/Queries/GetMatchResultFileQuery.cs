using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.Application.MatchExecutions.Queries
{
	public class GetMatchResultFileQuery : IRequest<Stream>
	{
		public long Id { get; set; }

		public string Filename { get; set; }

		public class Validator : AbstractValidator<GetMatchResultFileQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Filename).Required().MinLength(1);
			}
		}

		public class Handler : IRequestHandler<GetMatchResultFileQuery, Stream>
		{
			private readonly IMediator mediator;

			public Handler(IMediator mediator)
			{
				this.mediator = mediator;
			}

			/// <inheritdoc />
			public async Task<Stream> Handle(GetMatchResultFileQuery request, CancellationToken cancellationToken)
			{
				if (Utils.IsMaskedFile(request.Filename)) 
					return null;

				await using var archive =
					await mediator.Send(new GetMatchResultArchiveQuery(request.Id),
						cancellationToken);

				if (archive == null) return null;

				using var zip = new ZipArchive(archive, ZipArchiveMode.Read, true);

				var entry = zip.GetEntry(request.Filename);
				if (entry == null) return null;

				var ms = new MemoryStream();
				await using var contentStream = entry.Open();

				await contentStream.CopyToAsync(ms, cancellationToken);
				ms.Seek(0, SeekOrigin.Begin);
				return ms;
			}
		}
	}
}