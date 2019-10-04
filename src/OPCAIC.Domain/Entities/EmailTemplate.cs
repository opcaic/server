namespace OPCAIC.Domain.Entities
{
	public class EmailTemplate : Entity
	{
		public string Name { get; set; }

		public string LanguageCode { get; set; }

		public string SubjectTemplate { get; set; }

		public string BodyTemplate { get; set; }
	}
}