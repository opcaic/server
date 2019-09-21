using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Exceptions;
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

		[Fact]
		public async Task GetByFilter_Success()
		{
			documentRepository.Setup(r
					=> r.GetByFilterAsync(It.IsAny<DocumentFilterDto>(), CancellationToken))
				.ReturnsAsync(new ListDto<DocumentDetailDto>
				{
					Total = 1,
					List = new List<DocumentDetailDto>
					{
						new DocumentDetailDto
						{
							Tournament = new TournamentReferenceDto {Id = TournamentId}
						}
					}
				});

			var list = await DocumentService.GetByFilterAsync(new DocumentFilterModel {Count = 1},
				CancellationToken);

			list.Total.ShouldBe(1);
			list.List.ShouldHaveSingleItem();
		}

		[Fact]
		public async Task Manage_NotExistingIds()
		{
			var fakeId = 5;

			tournamentRepository.Setup(r => r.ExistsByIdAsync(fakeId, CancellationToken))
				.ReturnsAsync(false);
			var exception = await Should.ThrowAsync<NotFoundException>(() =>
				DocumentService.CreateAsync(new NewDocumentModel {TournamentId = fakeId},
					CancellationToken));
			exception.Resource.ShouldBe(nameof(Tournament));

			documentRepository.Setup(r => r.FindByIdAsync(fakeId, CancellationToken))
				.ReturnsAsync(default(DocumentDetailDto));
			exception = await Should.ThrowAsync<NotFoundException>(() =>
				DocumentService.GetByIdAsync(fakeId, CancellationToken));
			exception.Resource.ShouldBe(nameof(Document));
		}
	}
}