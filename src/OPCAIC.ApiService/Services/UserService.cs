using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class UserService : IUserService
	{
		private readonly IConfiguration configuration;
		private readonly IMapper mapper;
		private readonly IUserRepository userRepository;
		private readonly IUserTournamentRepository userTournamentRepository;

		private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

		public UserService(IConfiguration configuration, IMapper mapper, IUserRepository userRepository, IUserTournamentRepository userTournamentRepository)
		{
			this.configuration = configuration;
			this.mapper = mapper;
			this.userRepository = userRepository;
			this.userTournamentRepository = userTournamentRepository;
		}

		public async Task<long> CreateAsync(NewUserModel user, CancellationToken cancellationToken)
		{
			if (await userRepository.ExistsByEmailAsync(user.Email, cancellationToken))
			{
				throw new ConflictException("user-email-conflict");
			}

			var dto = mapper.Map<NewUserDto>(user);

			return await userRepository.CreateAsync(dto, cancellationToken);

#warning TODO - Send verification email
		}

		public async Task<ListModel<UserPreviewModel>> GetByFilterAsync(UserFilterModel filter, CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<UserFilterDto>(filter);

			var dto = await userRepository.GetByFilterAsync(filterDto, cancellationToken);

			return new ListModel<UserPreviewModel>
			{
				Total = dto.Total,
				List = dto.List.Select(user => mapper.Map<UserPreviewModel>(user))
			};
		}

		public async Task<UserDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			var dto = await userRepository.FindByIdAsync(id, cancellationToken);
			if (dto == null)
				throw new NotFoundException(nameof(User), id);

			return mapper.Map<UserDetailModel>(dto);
		}

		public async Task UpdateAsync(long id, UserProfileModel model, CancellationToken cancellationToken)
		{
			var dto = mapper.Map<UserProfileDto>(model);

			if (!await userRepository.UpdateAsync(id, dto, cancellationToken))
				throw new NotFoundException(nameof(User), id);
		}

		public async Task<UserIdentityModel> AuthenticateAsync(string email, string passwordHash,
			CancellationToken cancellationToken)
		{
			var user = await userRepository.AuthenticateAsync(email, passwordHash, cancellationToken);
			if (user == null)
			{
				return null;
			}

			var conf = configuration.GetSecurityConfiguration();

			var tournamentIds = await userTournamentRepository.FindTournamentsByUserAsync(user.Id, cancellationToken);

			var claim = new Claim(RolePolicy._PolicyName, ((UserRole)user.RoleId).ToString());
			string accessToken = CreateToken(conf.Key, TimeSpan.FromMinutes(conf.AccessTokenExpirationSeconds), claim);

			var refreshTokenClaim = new Claim("user", user.Id.ToString());
			string refreshToken = CreateToken(conf.Key, TimeSpan.FromDays(conf.RefreshTokenExpirationDays), refreshTokenClaim);

			return new UserIdentityModel
			{
				Id = user.Id,
				Email = user.Email,
				Role = (UserRole)user.RoleId,
				RefreshToken = refreshToken,
				AccessToken = accessToken,
				ManagedTournamentIds = tournamentIds
			};
		}

		private static TokenValidationParameters GetValidationParameters(string key)
		{
			return new TokenValidationParameters()
			{
				ValidateLifetime = true, 
				ValidateAudience = false, 
				ValidateIssuer = false, 
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) 
			};
		}

		public async Task<UserTokens> RefreshTokens(long userId, string oldToken, CancellationToken cancellationToken)
		{
			var conf = configuration.GetSecurityConfiguration();

			try
			{
				var principal = tokenHandler.ValidateToken(oldToken, GetValidationParameters(conf.Key), out var token);
			}
			catch (Exception ex)
			{
				throw new UnauthorizedExcepion(ex.Message);
			}

			var identity = await userRepository.FindIdentityAsync(userId, cancellationToken);
							   
			var refreshTokenClaim = new Claim("user", userId.ToString());
			var newToken = CreateToken(conf.Key, TimeSpan.FromDays(conf.RefreshTokenExpirationDays), refreshTokenClaim);

			var claim = new Claim(RolePolicy._PolicyName, ((UserRole)identity.RoleId).ToString());
			var accessToken = CreateToken(conf.Key, TimeSpan.FromSeconds(conf.AccessTokenExpirationSeconds), claim);

			return new UserTokens
			{
				RefreshToken = newToken,
				AccessToken = accessToken
			};
		}

		private string CreateToken(string key, TimeSpan expiresIn, params Claim[] claims)
		{
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.Now.Add(expiresIn),
				SigningCredentials = new SigningCredentials(CreateSymmetricKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

		private SecurityKey CreateSymmetricKey(string key)
		{
			return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
		}
	}
}