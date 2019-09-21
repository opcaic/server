using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Exceptions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class TournamentInvitationsServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public TournamentInvitationsServiceTest(ITestOutputHelper output) : base(output)
		{
			Services.AddMapper();
			urlGenerator = Services.Mock<IFrontendUrlGenerator>(MockBehavior.Strict);
			emailService = Services.Mock<IEmailService>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			tournamentInvitationRepository =
				Services.Mock<ITournamentInvitationRepository>(MockBehavior.Strict);
			Services.AddTransient<TournamentInvitationsService>();
		}

		private readonly Mock<IFrontendUrlGenerator> urlGenerator;
		private readonly Mock<IEmailService> emailService;
		private readonly Mock<ITournamentRepository> tournamentRepository;
		private readonly Mock<ITournamentInvitationRepository> tournamentInvitationRepository;

		[Fact]
		public async Task Create_Success()
		{
			tournamentRepository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken))
				.ReturnsAsync(new TournamentDetailDto {Name = "tournament"});

			tournamentInvitationRepository
				.Setup(r => r.GetInvitationsAsync(It.IsAny<long>(),
					It.IsAny<TournamentInvitationFilterDto>(), CancellationToken))
				.ReturnsAsync(new ListDto<TournamentInvitationDto>
				{
					Total = 1,
					List = new List<TournamentInvitationDto>
					{
						new TournamentInvitationDto {Id = 1, Email = "mail@mail.cz"}
					}
				});

			tournamentInvitationRepository
				.Setup(r => r.CreateAsync(It.IsAny<long>(), It.IsAny<IEnumerable<string>>(),
					CancellationToken))
				.ReturnsAsync(true);

			urlGenerator
				.Setup(r => r.TournamentPageLink(It.IsAny<long>()))
				.Returns("url");

			var newmail = "newmail@mail.cz";
			emailService
				.Setup(r => r.EnqueueEmailAsync(It.IsAny<EmailDtoBase>(), newmail,
					CancellationToken))
				.Returns(Task.CompletedTask);

			await GetService<TournamentInvitationsService>()
				.CreateAsync(0, new List<string> {newmail}, CancellationToken);
		}

		[Fact]
		public async Task Delete_Failure()
		{
			tournamentRepository
				.Setup(r => r.ExistsByIdAsync(It.IsAny<long>(), CancellationToken))
				.ReturnsAsync(false);

			var exception = await Should.ThrowAsync<NotFoundException>(()
				=> GetService<TournamentInvitationsService>()
					.DeleteAsync(0, It.IsAny<string>(), CancellationToken));
			exception.Resource.ShouldBe(nameof(Tournament));
			exception.ResourceId.ShouldBe(0);

			tournamentRepository
				.Setup(r => r.ExistsByIdAsync(It.IsAny<long>(), CancellationToken))
				.ReturnsAsync(true);
			tournamentInvitationRepository
				.Setup(r => r.DeleteAsync(It.IsAny<long>(), It.IsAny<string>(), CancellationToken))
				.ReturnsAsync(false);

			var exception2 = await Should.ThrowAsync<ConflictException>(()
				=> GetService<TournamentInvitationsService>()
					.DeleteAsync(0, "mail", CancellationToken));
		}

		[Fact]
		public async Task GetInvitations_Success()
		{
			tournamentRepository
				.Setup(r => r.ExistsByIdAsync(It.IsAny<long>(), CancellationToken))
				.ReturnsAsync(true);

			tournamentInvitationRepository
				.Setup(r => r.GetInvitationsAsync(It.IsAny<long>(),
					It.IsAny<TournamentInvitationFilterDto>(), CancellationToken))
				.ReturnsAsync(new ListDto<TournamentInvitationDto>
				{
					Total = 1,
					List = new List<TournamentInvitationDto>
					{
						new TournamentInvitationDto {Id = 1}
					}
				});

			var result = await GetService<TournamentInvitationsService>()
				.GetInvitationsAsync(0, default, CancellationToken);
			result.Total.ShouldBe(1);
			result.List[0].Id.ShouldBe(1);
		}
	}
}