using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Security;

namespace OPCAIC.ApiService.Services
{
	public class UserService : IUserService
	{
		private readonly Dictionary<string, UserIdentity> fUsers;

		public UserService()
		{
			// TODO mock data, replace with DB configuration
			fUsers = new Dictionary<string, UserIdentity>();
			fUsers.Add("test@user.com",
				new UserIdentity {Id = 1, Email = "test@user.com", PasswordHash = "password"});
			fUsers.Add("example@user.com",
				new UserIdentity {Id = 2, Email = "example@user.com", PasswordHash = "password"});
		}

		public async Task<UserIdentity[]> GetAllAsync()
			=> await Task.FromResult(fUsers.Values.ToArray());

		public Task<UserIdentity> Authenticate(string email, string passwordHash)
		{
			var user = FindByEmail(email);
			if (user == null || user.PasswordHash != passwordHash)
			{
				return null;
			}

			var jwtTokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(
				Environment.GetEnvironmentVariable(EnvVariables.SecurityKey));
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] {new Claim("user", user.Id.ToString())}),
				Expires = DateTime.Now.AddHours(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256Signature)
			};
			var token = jwtTokenHandler.CreateToken(tokenDescriptor);

			return Task.FromResult(new UserIdentity
			{
				Id = user.Id,
				Email = user.Email,
				Role = user.Role,
				Token = jwtTokenHandler.WriteToken(token)
			});
		}

		private UserIdentity FindByEmail(string email)
		{
			if (!fUsers.TryGetValue(email, out var user))
			{
				user = null;
			}

			return user;
		}
	}
}
