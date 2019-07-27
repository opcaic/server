using AutoMapper;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos.Users;
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
			exp.CreateMap<User, UserPreviewDto>().ForMember(usr => usr.UserRole, opt => opt.MapFrom(usr => usr.RoleId));
			exp.CreateMap<User, UserDetailDto>().ForMember(usr => usr.UserRole, opt => opt.MapFrom(usr => usr.RoleId));

			exp.CreateMap<UserPreviewDto, UserPreviewModel>();
			exp.CreateMap<UserDetailDto, UserDetailModel>();

			exp.CreateMap<UserProfileModel, UserProfileDto>();
			exp.CreateMap<UserFilterModel, UserFilterDto>();
		}
	}
}