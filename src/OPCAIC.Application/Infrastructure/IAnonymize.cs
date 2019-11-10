namespace OPCAIC.Application.Infrastructure
{
	public interface IAnonymize
	{
		/// <summary>
		///     Force override of anonymization settings of respective tournament.
		/// </summary>
		bool? Anonymize { get; set; }
	}
}