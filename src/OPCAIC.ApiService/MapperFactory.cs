using AutoMapper;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService
{
	public static class MapperConfigurationFactory
	{
		public static MapperConfiguration Create() => new MapperConfiguration(exp =>
		{
			exp.AddUserMapping();
			exp.AddTournamentMapping();
			exp.AddSubmissionMapping();
			exp.AddMatchMapping();
		});

		private static void AddUserMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewUserModel, NewUserDto>()
				.ForMember(usr => usr.RoleId, opt => opt.Ignore())
				.ForMember(usr => usr.PasswordHash,
					opt => opt.MapFrom(usr => Hashing.HashPassword(usr.Password)));

			exp.CreateMap<User, UserIdentityDto>();
		}

		private static void AddTournamentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Tournament, TournamentInfoDto>();
		}

		private static void AddSubmissionMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Submission, SubmissionStorageDto>();
		}

		private static void AddMatchMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<MatchExecution, MatchExecutionStorageDto>();
		}
	}
}