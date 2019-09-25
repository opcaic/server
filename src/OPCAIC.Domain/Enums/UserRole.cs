namespace OPCAIC.Domain.Enums
{
	public enum UserRole
	{
		/// <summary>
		///     Unauthenticated user.
		/// </summary>
		None = 0,

		/// <summary>
		///     Common user in the platform.
		/// </summary>
		User = 1,

		/// <summary>
		///     Common user with elevated permissions.
		/// </summary>
		Organizer = 2,

		/// <summary>
		///     System administrator.
		/// </summary>
		Admin = 3
	}
}