using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Security
{
	public static class Hashing
	{
		private static readonly byte[] salt = new byte[128 / 8];
		private static readonly Random rnd = new Random(4242);

		static Hashing()
		{
			rnd.NextBytes(salt);
		}

		public static string HashPassword(string password)
		{
			Require.ArgNotNull(password, nameof(password));

			return Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password,
				salt,
				KeyDerivationPrf.HMACSHA1,
				10000,
				256 / 8));
		}
	}
}