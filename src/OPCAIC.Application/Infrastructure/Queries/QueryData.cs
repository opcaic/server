using System.Diagnostics;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.Infrastructure.Queries
{
	public class QueryData<T>
	{
		public T Dto { get; set; }
		public TournamentOrganizersDto OrganizersDto { get; set; }
		public bool TournamentAnonymize { get; set; }
	}

	public static class QueryDataExtensions
	{
		private static void AnonymizeInternal<T>(QueryData<T> data, long? userId, UserRole role, bool? doOverride)
			where T : IAnonymizable
		{
			// allow organizers to override anonymization settings
			var shouldAnonymize = !data.OrganizersDto.IsOrganizer(userId, role)
				? data.TournamentAnonymize
				: doOverride ?? data.TournamentAnonymize;

			if (shouldAnonymize)
			{
				data.Dto.AnonymizeUsersExcept(userId);
			}
		}

		public static void AnonymizeIfNecessary<TData>(this QueryData<TData> data, IPublicRequest request) where TData : IAnonymizable
		{
			Debug.Assert(data != null);
			Debug.Assert(request != null);

			AnonymizeInternal(data, request.RequestingUserId, request.RequestingUserRole, (request as IAnonymize)?.Anonymize);
		}

		public static void AnonymizeIfNecessary<T>(this QueryData<T> data, IAuthenticatedRequest request)
			where T : IAnonymizable
		{
			Debug.Assert(data != null);
			Debug.Assert(request != null);

			AnonymizeInternal(data, request.RequestingUserId, request.RequestingUserRole, (request as IAnonymize)?.Anonymize);
		}
	}
}