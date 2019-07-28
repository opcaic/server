namespace OPCAIC.Infrastructure.Enums
{
	public enum GameModuleEntryPointResult
	{
		/// <summary>
		///     Unknown result, should never occur.
		/// </summary>
		NotExecuted,

		/// <summary>
		///     Stage of the game module finished successfully, the result of the stage is Success.
		/// </summary>
		Success,

		/// <summary>
		///     Stage of the game module finished successfully, the result of the stage is Failure.
		/// </summary>
		UserError,

		/// <summary>
		///     Stage of the game module failed during runtime because of error in game module.
		/// </summary>
		ModuleError,

		/// <summary>
		///     Stage of the game module failed during runtime because of error in the platform.
		/// </summary>
		PlatformError
	}
}