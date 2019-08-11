using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class UserService : IUserService
	{
		private readonly IConfiguration configuration;
		private readonly IMapper mapper;
		private readonly ITokenService tokenService;
		private readonly IUserRepository userRepository;
		private readonly IUserTournamentRepository userTournamentRepository;

		public UserService(IConfiguration configuration, IMapper mapper, ITokenService tokenService, IUserRepository userRepository, IUserTournamentRepository userTournamentRepository)
		{
			this.configuration = configuration;
			this.mapper = mapper;
			this.tokenService = tokenService;
			this.userRepository = userRepository;
			this.userTournamentRepository = userTournamentRepository;
		}

		public async Task<long> CreateAsync(NewUserModel user, CancellationToken cancellationToken)
		{
			if (await userRepository.ExistsByEmailAsync(user.Email, cancellationToken))
			{
				throw new ConflictException("user-email-conflict");
			}

			if (await userRepository.ExistsByUsernameAsync(user.Username, cancellationToken))
			{
				throw new ConflictException("user-username-conflict");
			}

			var dto = mapper.Map<NewUserDto>(user);

			return await userRepository.CreateAsync(dto, cancellationToken);
		}

		public async Task<ListModel<UserPreviewModel>> GetByFilterAsync(UserFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<UserFilterDto>(filter);

			var dto = await userRepository.GetByFilterAsync(filterDto, cancellationToken);

			return new ListModel<UserPreviewModel>
			{
				Total = dto.Total,
				List = dto.List.Select(user => mapper.Map<UserPreviewModel>(user))
			};
		}

		public async Task<UserDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			var dto = await userRepository.FindByIdAsync(id, cancellationToken);
			if (dto == null)
			{
				throw new NotFoundException(nameof(User), id);
			}

			return mapper.Map<UserDetailModel>(dto);
		}

		public async Task UpdateAsync(long id, UserProfileModel model,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<UserProfileDto>(model);

			if (!await userRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(User), id);
			}
		}

		public async Task<UserIdentityModel> AuthenticateAsync(string email, string passwordHash,
			CancellationToken cancellationToken)
		{
			var user =
				await userRepository.AuthenticateAsync(email, passwordHash, cancellationToken);
			if (user == null)
			{
				return null;
			}

			var conf = configuration.GetSecurityConfiguration();

			var claim = new Claim(RolePolicy.PolicyName, ((UserRole)user.RoleId).ToString());
			var accessToken = tokenService.CreateToken(conf.Key, TimeSpan.FromMinutes(conf.AccessTokenExpirationMinutes), claim);

			var refreshTokenClaim = new Claim("user", user.Id.ToString());
			var refreshToken = tokenService.CreateToken(conf.Key, TimeSpan.FromDays(conf.RefreshTokenExpirationDays), refreshTokenClaim);

			return new UserIdentityModel
			{
				Id = user.Id,
				Email = user.Email,
				Role = (UserRole)user.RoleId,
				RefreshToken = refreshToken,
				AccessToken = accessToken
			};
		}

		public async Task<UserTokens> RefreshTokens(long userId, string oldToken,
			CancellationToken cancellationToken)
		{
			var conf = configuration.GetSecurityConfiguration();

			if (tokenService.ValidateToken(conf.Key, oldToken) == null)
				throw new UnauthorizedExcepion("invalid-token");

			var identity = await userRepository.FindIdentityAsync(userId, cancellationToken);

			var refreshTokenClaim = new Claim("user", userId.ToString());
			var newToken = tokenService.CreateToken(conf.Key, TimeSpan.FromDays(conf.RefreshTokenExpirationDays), refreshTokenClaim);

			var claim = new Claim(RolePolicy.PolicyName, ((UserRole)identity.RoleId).ToString());
			var accessToken = tokenService.CreateToken(conf.Key, TimeSpan.FromSeconds(conf.AccessTokenExpirationMinutes), claim);

			return new UserTokens {RefreshToken = newToken, AccessToken = accessToken};
		}

		public async Task<string> CreateResetUrlAsync(string email, CancellationToken cancellationToken)
		{
			string key = Hashing.CreateKey(32);

			if (!await userRepository.UpdatePasswordKeyAsync(email, key, cancellationToken))
				throw new BadRequestException($"User with email {email} was not found.");

			string appBaseUrl = configuration.GetAppBaseUrl();

			return $"{appBaseUrl}/passwordReset?email={email}&key={key}";
		}

		public async Task UpdatePasswordAsync(string email, NewPasswordModel model, CancellationToken cancellationToken)
		{
			var passwordData = await userRepository.FindPasswordDataAsync(email, cancellationToken);
			if (passwordData == null)
				throw new BadRequestException($"User with email {email} was not found.");
			
			if (model.OldPassword != null)
			{
				if (Hashing.HashPassword(model.OldPassword) != passwordData.PasswordHash)
					throw new ConflictException("old-password-conflict");
			}
			else
			{
				if (model.PasswordKey != passwordData.PasswordKey)
					throw new ConflictException("password-key-conflict");
			}

			await userRepository.UpdatePasswordDataAsync(passwordData.Id, new UserPasswordDto
			{
				PasswordHash = Hashing.HashPassword(model.NewPassword),
				PasswordKey = null
			}, cancellationToken);
		}

		public async Task TryVerifyEmailAsync(string email, string token, CancellationToken cancellationToken)
		{
			var conf = configuration.GetSecurityConfiguration();

			var claims = tokenService.ValidateToken(conf.Key, token);
			if (claims == null || !claims.Any(claim => claim.Type == "email" && claim.Value == email))
				throw new BadRequestException("Invalid verification token.");

			if (!await userRepository.UpdateEmailVerifiedAsync(email, true, cancellationToken))
				throw new ConflictException("User with given email was not found.");
		}
	}
}