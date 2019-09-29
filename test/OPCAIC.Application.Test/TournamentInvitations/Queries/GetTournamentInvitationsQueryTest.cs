using System.Threading;
using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.TournamentInvitations.Models;
using OPCAIC.Application.TournamentInvitations.Queries;
using OPCAIC.Domain.Entities;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.TournamentInvitations.Queries
{
	public class GetTournamentInvitationsQueryTest : HandlerTest<GetTournamentInvitationsQuery.Handler>
	{
		/// <inheritdoc />
		public GetTournamentInvitationsQueryTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<TournamentInvitation>>();
		}

		private readonly Mock<IRepository<TournamentInvitation>> repository;

		[Fact]
		public async Task Success()
		{
			var invitations = Faker.Configure<TournamentInvitationDto>()
				.RuleFor(m => m.Email, f => f.Internet.Email())
				.RuleFor(m => m.Id, f => f.IndexFaker).Generate(10);

			repository.Setup(r => r.ListPagedAsync(It.IsAny<ProjectingSpecification<
					TournamentInvitation,
					TournamentInvitationDto>>(), CancellationToken.None))
				.ReturnsAsync(
					new PagedResult<TournamentInvitationDto>(invitations.Count, invitations));

			var list = await Handler.Handle(new GetTournamentInvitationsQuery
			{
				Count = 10
			}, CancellationToken.None);

			list.Total.ShouldBe(10);
			list.List.Count.ShouldBe(10);
		}
	}
}