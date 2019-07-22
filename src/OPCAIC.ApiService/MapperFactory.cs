using AutoMapper;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService
{
	public static class MapperConfigurationFactory
	{
		public static MapperConfiguration Create()
		{
			return new MapperConfiguration(exp =>
			{
				exp.AddUserMapping();
			});
		}

		private static void AddUserMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewUserModel, NewUserDto>()
				.ForMember(usr => usr.RoleId, opt => opt.Ignore())
				.ForMember(usr => usr.PasswordHash,
					opt => opt.MapFrom(usr => Hashing.HashPassword(usr.Password)));

			exp.CreateMap<User, UserIdentityDto>();
		}
	}
}