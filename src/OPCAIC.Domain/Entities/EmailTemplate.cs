namespace OPCAIC.Infrastructure.Entities
{
	public class EmailTemplate : EntityBase
	{
		public string Name { get; set; }

		public string LanguageCode { get; set; }

		public string SubjectTemplate { get; set; }

		public string BodyTemplate { get; set; }
	}
}