using System;
using System.ComponentModel;
using System.Globalization;

namespace OPCAIC.Domain.Infrastructure
{
	public class EnumerationConverter<T> : TypeConverter
		where T : Enumeration<T>, new()
	{
		/// <inheritdoc />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || sourceType == typeof(int);
		}

		/// <inheritdoc />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || destinationType == typeof(int);
		}

		/// <inheritdoc />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			switch (value)
			{
				case string s:
					return Enumeration<T>.FromName(s);
				case int i:
					return Enumeration<T>.FromId(i);
			}

			return null;
		}

		/// <inheritdoc />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
			Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return (value as Enumeration<T>)?.Name;
			}
			if (destinationType == typeof(int))
			{
				return (value as Enumeration<T>)?.Id;
			}

			return null;
		}
	}
}