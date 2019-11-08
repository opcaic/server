using System.Collections.Generic;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Application.Dtos.Users
{
	public class UserReferenceDto : ValueObject
	{
		public static readonly UserReferenceDto Anonymous = null;

		public long Id { get; set; }

		public string Username { get; set; }

		public string Organization { get; set; }

		/// <inheritdoc />
		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Id;
			yield return Username;
			yield return Organization;
		}
	}
}