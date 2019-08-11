using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OPCAIC.ApiService.Services
{
	public class TokenService : ITokenService
	{
		private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

		private static TokenValidationParameters GetValidationParameters(string key)
			=> new TokenValidationParameters
			{
				ValidateLifetime = true,
				ValidateAudience = false,
				ValidateIssuer = false,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
			};

		public string CreateToken(string key, TimeSpan? expiresIn, params Claim[] claims)
		{
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = expiresIn != null ? DateTime.Now.Add(expiresIn.Value) : (DateTime?)null,
				SigningCredentials = new SigningCredentials(CreateSymmetricKey(key),
					SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

		private SecurityKey CreateSymmetricKey(string key)
			=> new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

		public IEnumerable<Claim> ValidateToken(string key, string token)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			try
			{
				tokenHandler.ValidateToken(token, GetValidationParameters(key), out var securityToken);
				var jwtToken = securityToken as JwtSecurityToken;
				return jwtToken.Claims;
			}
			catch (SecurityTokenException)
			{
				return null;
			}
		}
	}
}
