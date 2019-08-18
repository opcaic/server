using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Services
{
	public class JwtTokenProvider : IUserTwoFactorTokenProvider<User>
	{
		public const string RefreshPurpose = "Refresh";
		public const string AccessPurpose = "Access";
		public const string SecurityStampClaim = "stamp";

		private readonly JwtIssuerOptions jwtOptions;
		private readonly ILogger<JwtTokenProvider> logger;
		private readonly SecurityConfiguration securityConfig;
		private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

		public JwtTokenProvider(IOptions<JwtIssuerOptions> jwtOptions,
			IOptions<SecurityConfiguration> securityConfig, ILogger<JwtTokenProvider> logger)
		{
			this.logger = logger;
			this.jwtOptions = jwtOptions.Value;
			this.securityConfig = securityConfig.Value;
		}

		/// <inheritdoc />
		public Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
		{
			switch (purpose)
			{
				case AccessPurpose:
					return CreateJwtAccessToken(manager, user);
				case RefreshPurpose:
					return ResetJwtRefreshToken(manager, user);
				default:
					throw new ArgumentOutOfRangeException(nameof(purpose));
			}
		}

		/// <inheritdoc />
		public Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager,
			User user)
		{
			switch (purpose)
			{
				// Access token is validated by JWT middleware, only need to validate refresh token
				case RefreshPurpose:
					return ValidateJwtRefreshToken(manager, user, token);
				default:
					throw new ArgumentOutOfRangeException(nameof(purpose));
			}
		}

		/// <inheritdoc />
		public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user)
		{
			return Task.FromResult(true);
		}

		private Task<bool> ValidateJwtRefreshToken(UserManager<User> manager, User user,
			string token)
		{
			if (token == null)
			{
				throw new ArgumentNullException(nameof(token));
			}

			try
			{
				var principal = ExtractClaimsPrincipal(token);

				return Task.FromResult(
					principal != null &&
					user.UserName == principal.FindFirstValue(ClaimTypes.Name) &&
					GetHashedSecurityStamp(user) == principal.FindFirstValue(SecurityStampClaim));
			}
			catch (Exception e)
			{
				logger.LogWarning(LoggingEvents.JwtRefreshTokenValidationFailed, e,
					$"An error occured when validating refresh token {{{LoggingTags.RefreshToken}}} of {{{LoggingTags.UserId}}}",
					user.Id);
			}

			return Task.FromResult(false);
		}

		private ClaimsPrincipal ExtractClaimsPrincipal(string token)
		{
			try
			{
				return tokenHandler.ValidateToken(token, GetValidationParameters(), out _);
			}
			catch (ArgumentException)
			{
				// malformed token
				return null;
			}
		}

		private async Task<string> CreateJwtAccessToken(UserManager<User> manager, User user)
		{
			return CreateToken(TimeSpan.FromMinutes(securityConfig.AccessTokenExpirationMinutes),
				new ClaimsIdentity(await manager.GetClaimsAsync(user)));
		}

		private async Task<string> ResetJwtRefreshToken(UserManager<User> manager, User user)
		{
			var nameClaim = new Claim(ClaimTypes.Name, user.UserName);
			var stampClaim = new Claim(SecurityStampClaim, GetHashedSecurityStamp(user));
			var token = CreateToken(TimeSpan.FromDays(securityConfig.RefreshTokenExpirationDays),
				new ClaimsIdentity(new[] {nameClaim, stampClaim}));


			return token;
		}

		private string CreateToken(TimeSpan? expiresIn, ClaimsIdentity identity)
		{
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = identity,
				Expires =
					expiresIn != null ? DateTime.Now.Add(expiresIn.Value) : (DateTime?)null,
				Audience = jwtOptions.Audience,
				Issuer = jwtOptions.Issuer,
				SigningCredentials = jwtOptions.SigningCredentials
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

		private TokenValidationParameters GetValidationParameters()
		{
			return new TokenValidationParameters
			{
				ValidateLifetime = true,
				ValidateAudience = false,
				ValidateIssuer = false,
				IssuerSigningKey = jwtOptions.SigningCredentials.Key
			};
		}

		private string GetHashedSecurityStamp(User user)
		{
			var sha = SHA1.Create();
			var bytes = Encoding.Default.GetBytes(user.SecurityStamp);
			var hash = sha.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}
	}
}