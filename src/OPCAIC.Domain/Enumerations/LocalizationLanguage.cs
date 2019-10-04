using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Domain.Enumerations
{
	public class LocalizationLanguage : Enumeration<LocalizationLanguage>
	{
		public static readonly LocalizationLanguage EN = Create("en");
		public static readonly LocalizationLanguage CZ = Create("cs");

		public static implicit operator string(LocalizationLanguage lang)
		{
			return lang.Name;
		}
	}
}