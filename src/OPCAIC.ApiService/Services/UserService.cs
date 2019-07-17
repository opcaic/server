﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class UserService : IUserService
	{
		private readonly IConfiguration configuration;
		private readonly IUserRepository userRepository;
		private readonly IUserTournamentRepository userTournamentRepository;

		public UserService(IConfiguration configuration, IUserRepository userRepository, IUserTournamentRepository userTournamentRepository)
		{
			this.configuration = configuration;
			this.userRepository = userRepository;
			this.userTournamentRepository = userTournamentRepository;
		}

		public async Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken)
		{
			if (await userRepository.ExistsByEmailAsync(user.Email, cancellationToken))
				throw new ConflictException("user-email-conflict");

			return await userRepository.CreateAsync(user, cancellationToken);

#warning TODO - Send verification email
		}

		public Task<UserIdentityDto[]> GetAllAsync(CancellationToken cancellationToken)
		{
			return userRepository.GetAsync(cancellationToken);
		}

		public async Task<UserIdentity> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken)
		{
			var user = await userRepository.AuthenticateAsync(email, passwordHash, cancellationToken);
			if (user == null)
				return null;

			var jwtTokenHandler = new JwtSecurityTokenHandler();

			string key = configuration.GetValue<string>(ConfigNames.SecurityKey);

			var tournamentIds = await userTournamentRepository.FindTournamentsByUserAsync(user.Id, cancellationToken);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] { new Claim("role", user.RoleId.ToString()) }),
				Expires = DateTime.Now.AddHours(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
					SecurityAlgorithms.HmacSha256Signature)
			};
			var token = jwtTokenHandler.CreateToken(tokenDescriptor);

			return new UserIdentity
			{
				Id = user.Id,
				Email = user.Email,
				Role = (UserRole)user.RoleId,
				Token = jwtTokenHandler.WriteToken(token),
				ManagedTournamentIds = tournamentIds
			};
		}
	}
}
