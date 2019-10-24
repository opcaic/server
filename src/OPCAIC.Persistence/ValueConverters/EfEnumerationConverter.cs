using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Persistence.ValueConverters
{
	public class EfEnumerationConverter<T> : ValueConverter<T, string>
		where T: Enumeration<T>
	{
		/// <inheritdoc />
		public EfEnumerationConverter(ConverterMappingHints hints = null) : base(t => t.Name, name => Enumeration<T>.FromName(name), hints)
		{
		}
	}
}