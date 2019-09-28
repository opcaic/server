using System;
using System.Collections.Generic;

namespace OPCAIC.ApiService.Serialization
{
	// adapted from https://gist.github.com/StevenLiekens/82ddcf1823ee91cf6d5edfcdb1f6a591

	/// <summary>
	///     Extend this class to configure a type with a discriminator field.
	/// </summary>
	public class DiscriminatorOptions
	{
		/// <summary>Gets the base type, which is typically (but not necessarily) abstract.</summary>
		public Type BaseType { get; set; }

		/// <summary>Gets the name of the discriminator field.</summary>
		public string DiscriminatorFieldName { get; set; }

		/// <summary>Returns true if the discriminator should be serialized to the CLR type; otherwise false.</summary>
		public bool SerializeDiscriminator { get; set; }

		/// <summary>Gets the mappings from discriminator values to CLR types.</summary>
		public List<(string Discriminator, Type Type)> DiscriminatedTypes { get; } = new List<(string Discriminator, Type Type)>();
	}
}