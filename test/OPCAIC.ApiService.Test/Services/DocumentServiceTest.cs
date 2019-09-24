using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Documents.Queries;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class DocumentServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public DocumentServiceTest(ITestOutputHelper output) : base(output)
		{
			Services.AddMapper();
			documentRepository = Services.Mock<IDocumentRepository>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);

			Faker.Configure<Tournament>()
				.RuleFor(t => t.Id, TournamentId);
		}

		private readonly Mock<IDocumentRepository> documentRepository;
		private readonly Mock<ITournamentRepository> tournamentRepository;

		public long TournamentId = 1;
		public long DocumentId = 1;

		private DocumentService DocumentService => GetService<DocumentService>();

		[Fact]
		public async Task Create_Success()
		{
			tournamentRepository.Setup(r => r.ExistsByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(true);

			documentRepository.Setup(r
					=> r.CreateAsync(It.Is<NewDocumentDto>(d => d.TournamentId == TournamentId),
						CancellationToken))
				.ReturnsAsync(DocumentId);

			var id = await DocumentService.CreateAsync(
				new NewDocumentModel
				{
					TournamentId = TournamentId,
					Content = "this is a test document",
					Name = "TEST"
				}, CancellationToken);
			id.ShouldBe(DocumentId);
		}
	}
}