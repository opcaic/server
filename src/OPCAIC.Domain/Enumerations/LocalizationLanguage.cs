using System.ComponentModel;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Domain.Enumerations
{
	[TypeConverter(typeof(EnumerationConverter<LocalizationLanguage>))]
	public class LocalizationLanguage : Enumeration<LocalizationLanguage>
	{
		public static readonly LocalizationLanguage EN = Create<LocalizationLanguage>("en");
		public static readonly LocalizationLanguage CZ = Create<LocalizationLanguage>("cs");

		public static implicit operator string(LocalizationLanguage lang)
		{
			return lang.Name;
		}
	}
}