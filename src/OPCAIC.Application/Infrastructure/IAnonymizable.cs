namespace OPCAIC.Application.Infrastructure
{
	public interface IAnonymizable
	{
		void AnonymizeUsersExcept(long? userId);
	}
}