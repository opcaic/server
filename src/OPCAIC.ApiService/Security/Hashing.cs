using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using OPCAIC.Utils;
using System;

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
				password: password,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA1,
				iterationCount: 10000,
				numBytesRequested: 256 / 8));
		}
	}
}
