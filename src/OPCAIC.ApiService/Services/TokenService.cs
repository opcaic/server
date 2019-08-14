using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OPCAIC.ApiService.Services
{
	public class TokenService : ITokenService
	{
		private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

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

		public IEnumerable<Claim> ValidateToken(string key, string token)
		{
			if (token == null)
			{
				throw new ArgumentNullException(nameof(token));
			}

			try
			{
				tokenHandler.ValidateToken(token, GetValidationParameters(key),
					out var securityToken);
				var jwtToken = securityToken as JwtSecurityToken;
				return jwtToken.Claims;
			}
			catch (SecurityTokenException)
			{
				return null;
			}
		}

		private static TokenValidationParameters GetValidationParameters(string key)
		{
			return new TokenValidationParameters
			{
				ValidateLifetime = true,
				ValidateAudience = false,
				ValidateIssuer = false,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
			};
		}

		private SecurityKey CreateSymmetricKey(string key)
		{
			return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
		}
	}
}