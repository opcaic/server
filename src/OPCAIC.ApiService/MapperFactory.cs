using AutoMapper;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Dtos.Emails;
using OPCAIC.Infrastructure.Dtos.EmailTemplates;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;
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
			exp.AddDocumentMapping();
			exp.AddMatchMapping();
			exp.AddEmailMapping();
			exp.AddEmailTemplateMapping();
			exp.AddGameMapping();
		});

		private static void AddDocumentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewDocumentModel, NewDocumentDto>();

			exp.CreateMap<Document, DocumentDetailDto>();
			exp.CreateMap<DocumentDetailDto, DocumentDetailModel>();

			exp.CreateMap<UpdateDocumentModel, UpdateDocumentDto>();
			exp.CreateMap<DocumentFilterModel, DocumentFilterDto>();
		}

		private static void AddUserMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewUserModel, NewUserDto>()
				.ForMember(usr => usr.RoleId, opt => opt.Ignore())
				.ForMember(usr => usr.PasswordHash,
					opt => opt.MapFrom(usr => Hashing.HashPassword(usr.Password)));

			exp.CreateMap<User, UserIdentityDto>();
			exp.CreateMap<User, UserPreviewDto>().ForMember(usr => usr.UserRole, 
        opt => opt.MapFrom(usr => usr.RoleId));
			exp.CreateMap<User, UserDetailDto>().ForMember(usr => usr.UserRole, 
        opt => opt.MapFrom(usr => usr.RoleId));
			exp.CreateMap<User, EmailRecipientDto>();
			exp.CreateMap<User, UserPasswordDto>();

			exp.CreateMap<UserPreviewDto, UserPreviewModel>();
			exp.CreateMap<UserDetailDto, UserDetailModel>();

			exp.CreateMap<UserProfileModel, UserProfileDto>();
			exp.CreateMap<UserFilterModel, UserFilterDto>();
		}

		private static void AddGameMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewGameModel, NewGameDto>();

			exp.CreateMap<Game, GamePreviewDto>();
			exp.CreateMap<Game, GameDetailDto>();

			exp.CreateMap<GamePreviewDto, GamePreviewModel>();
			exp.CreateMap<GameDetailDto, GameDetailModel>();

			exp.CreateMap<UpdateGameModel, UpdateGameDto>();
			exp.CreateMap<GameFilterModel, GameFilterDto>();
		}

		private static void AddTournamentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewTournamentModel, NewTournamentDto>();

			exp.CreateMap<Tournament, TournamentPreviewDto>();
			exp.CreateMap<Tournament, TournamentDetailDto>();
			exp.CreateMap<Tournament, TournamentReferenceDto>();

			exp.CreateMap<TournamentReferenceDto, TournamentReferenceModel>();

			exp.CreateMap<TournamentPreviewDto, TournamentPreviewModel>();
			exp.CreateMap<TournamentDetailDto, TournamentDetailModel>();

			exp.CreateMap<UpdateTournamentModel, UpdateTournamentDto>();
			exp.CreateMap<TournamentFilterModel, TournamentFilterDto>();

			exp.CreateMap<Game, GameReferenceDto>();
			exp.CreateMap<GameReferenceDto, GameReferenceModel>();
		}

		private static void AddSubmissionMapping(this IMapperConfigurationExpression exp)
			=> exp.CreateMap<Submission, SubmissionStorageDto>();

		private static void AddMatchMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<MatchExecution, MatchExecutionStorageDto>();
		}

		private static void AddEmailMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Email, EmailPreviewDto>();
		}

		private static void AddEmailTemplateMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<EmailTemplate, EmailTemplateDto>();
		}
	}
}