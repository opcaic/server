using System;
using System.Collections.Generic;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.ApiService.Utils
{
	public static class EnumerationConfig
	{
		private static readonly HashSet<Type> nameOnlySet = new HashSet<Type>()
		{
			typeof(LocalizationLanguage),
			typeof(EmailType)
		};

		public static IEnumerable<(Type type, bool preferName)> GetAnnotatedTypes()
		{
			foreach (var type in Enumeration.GetAllEnumerationTypes())
			{
				yield return (type, nameOnlySet.Contains(type));
			}
		}
	}
}