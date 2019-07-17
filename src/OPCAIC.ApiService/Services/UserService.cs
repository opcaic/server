using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Security;

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

		public async Task<UserIdentity[]> GetAllAsync()
			=> await Task.FromResult(fUsers.Values.ToArray());

		public async Task<UserIdentity> Authenticate(string email, string passwordHash)
		{
			var user = await userRepository.(email);
			if (user == null || user.PasswordHash != passwordHash)
			{
				return null;
			}

			var jwtTokenHandler = new JwtSecurityTokenHandler();

			string key = configuration.GetValue<string>(ConfigNames.SecurityKey);

			var tournamentIds = await userTournamentRepository.FindTournamentsByUserAsync(user.Id, cancellationToken);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] { new Claim("role", user.RoleId.ToString()) }),
				Expires = DateTime.Now.AddHours(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256Signature)
			};
			var token = jwtTokenHandler.CreateToken(tokenDescriptor);

			return Task.FromResult(new UserIdentity
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
