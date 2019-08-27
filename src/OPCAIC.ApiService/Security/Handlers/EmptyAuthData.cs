namespace OPCAIC.ApiService.Security.Handlers
{
	public class EmptyAuthData
	{
		public static readonly EmptyAuthData Instance = new EmptyAuthData();

		private EmptyAuthData()
		{
		}
	}
}