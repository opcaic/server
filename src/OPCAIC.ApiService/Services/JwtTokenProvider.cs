using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Services
{
	public class JwtTokenProvider : IUserTwoFactorTokenProvider<User>
	{
		private const string SecurityStampClaim = "stamp";
		public const string RefreshPurpose = "Refresh";
		public const string AccessPurpose = "Access";
		private readonly IJwtTokenService jwtTokenService;
		private readonly ILogger<JwtTokenProvider> logger;

		private readonly JwtConfiguration jwtConfig;

		public JwtTokenProvider(IOptions<JwtConfiguration> securityConfig,
			ILogger<JwtTokenProvider> logger, IJwtTokenService jwtTokenService)
		{
			this.jwtConfig = securityConfig.Value;
			this.logger = logger;
			this.jwtTokenService = jwtTokenService;
		}

		/// <inheritdoc />
		public Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
		{
			switch (purpose)
			{
				case AccessPurpose:
					return CreateJwtAccessToken(manager, user);
				case RefreshPurpose:
					return CreateJwtRefreshToken(manager, user);
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
				var principal = jwtTokenService.ExtractClaimsPrincipal(token);

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

		private async Task<string> CreateJwtAccessToken(UserManager<User> manager, User user)
		{
			var identity = new ClaimsIdentity(await manager.GetClaimsAsync(user));
			return jwtTokenService.CreateToken(
				TimeSpan.FromMinutes(jwtConfig.AccessTokenExpirationMinutes), identity);
		}

		private Task<string> CreateJwtRefreshToken(UserManager<User> manager, User user)
		{
			var nameClaim = new Claim(ClaimTypes.Name, user.UserName);
			var stampClaim = new Claim(SecurityStampClaim, GetHashedSecurityStamp(user));
			var identity = new ClaimsIdentity(new[] {nameClaim, stampClaim});
			var token =
				jwtTokenService.CreateToken(
					TimeSpan.FromDays(jwtConfig.RefreshTokenExpirationDays), identity);

			return Task.FromResult(token);
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