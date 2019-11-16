using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using OPCAIC.Application.Documents.Models;
using OPCAIC.Application.Documents.Queries;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.TestUtils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Documents.Queries
{
	public class GetDocumentsQueryTest : ServiceTestBase
	{
		/// <inheritdoc />
		public GetDocumentsQueryTest(ITestOutputHelper output) : base(output)
		{
			Services.AddAutoMapper(typeof(GetDocumentQuery).Assembly);
			documentRepository = Services.Mock<IRepository<Document>>();
		}

		private readonly Mock<IRepository<Document>> documentRepository;

		[Fact(Skip = "Until automapper conf is in Application project")]
		public async Task NotFound()
		{
			documentRepository.Setup(r
				=> r.FindAsync(It.IsAny<IProjectingSpecification<Document, DocumentDto>>(),
					CancellationToken.None)).ReturnsAsync((DocumentDto)null);

			var ex = await Should.ThrowAsync<NotFoundException>(()
				=> GetService<GetDocumentQuery.Handler>()
					.Handle(new GetDocumentQuery(1), CancellationToken.None));

			ex.Resource.ShouldBe(nameof(Document));
		}

		[Fact(Skip = "Until automapper conf is in Application project")]
		public async Task Success()
		{
			const long TournamentId = 1;

			documentRepository.Setup(r
					=> r.ListPagedAsync(It.IsAny<IProjectingSpecification<Document, DocumentDto>>(),
						CancellationToken.None))
				.ReturnsAsync(new PagedResult<DocumentDto>(
					1,
					new List<DocumentDto>
					{
						new DocumentDto
						{
							Tournament = new TournamentReferenceDto {Id = TournamentId}
						}
					}));

			var list = await GetService<GetDocumentsQuery.Handler>()
				.Handle(new GetDocumentsQuery {Count = 1}, CancellationToken.None);

			list.Total.ShouldBe(1);
			list.List.ShouldHaveSingleItem();
		}
	}
}