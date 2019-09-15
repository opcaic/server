using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Services
{
	public class UserManager : UserManager<User>, IUserManager
	{
		private readonly IMapper mapper;
		private readonly IUserRepository userRepository;

		/// <inheritdoc />
		public UserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor,
			IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators,
			IEnumerable<IPasswordValidator<User>> passwordValidators,
			ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
			IServiceProvider services, ILogger<UserManager> logger, IMapper mapper,
			IUserRepository userRepository, IOptions<SecurityConfiguration> securityConfiguration)
			: base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
				keyNormalizer, errors, services, logger)
		{
			this.mapper = mapper;
			this.userRepository = userRepository;
		}

		public Task<User> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
		}

		/// <inheritdoc />
		long IUserManager.GetUserId(ClaimsPrincipal principal)
		{
			return long.Parse(base.GetUserId(principal));
		}

		/// <inheritdoc />
		public override async Task<IList<Claim>> GetClaimsAsync(User user)
		{
			var claims = await base.GetClaimsAsync(user);

			claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer64));
			claims.Add(new Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.Integer64));
			claims.Add(new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email));
			claims.Add(new Claim(RolePolicy.UserRoleClaim, ((UserRole)user.RoleId).ToString()));

			return claims;
		}

		/// <inheritdoc />
		public async Task<ListModel<UserPreviewModel>> GetByFilterAsync(UserFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<UserFilterDto>(filter);

			var dto = await userRepository.GetByFilterAsync(filterDto, cancellationToken);

			return mapper.Map<ListModel<UserPreviewModel>>(dto);
		}

		/// <inheritdoc />
		public async Task<UserDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			var user = await FindByIdAsync(id, cancellationToken);
			if (user == null)
			{
				throw new NotFoundException(nameof(User), id);
			}

			return mapper.Map<UserDetailModel>(user);
		}

		/// <inheritdoc />
		public async Task UpdateAsync(long id, UserProfileModel model,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<UserProfileDto>(model);

			if (!await userRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(User), id);
			}
		}

		public async Task<UserTokens> GenerateUserTokensAsync(User user)
		{
			var accessToken = await GenerateUserTokenAsync(user,
				nameof(JwtTokenProvider), JwtTokenProvider.AccessPurpose);
			var refreshToken = await GenerateUserTokenAsync(user,
				nameof(JwtTokenProvider), JwtTokenProvider.RefreshPurpose);

			return new UserTokens { RefreshToken = refreshToken, AccessToken = accessToken };
		}

		public Task<bool> VerifyJwtRefreshToken(User user, string token)
		{
			return VerifyUserTokenAsync(user, nameof(JwtTokenProvider),
				JwtTokenProvider.RefreshPurpose, token);
		}
	}
}