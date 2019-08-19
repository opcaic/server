using AutoMapper;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Dtos.Emails;
using OPCAIC.Infrastructure.Dtos.EmailTemplates;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
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
				exp.AddTournamentMapping();
				exp.AddSubmissionMapping();
				exp.AddDocumentMapping();
				exp.AddMatchMapping();
				exp.AddEmailMapping();
				exp.AddEmailTemplateMapping();
				exp.AddGameMapping();
			});
		}

		private static void CreateMap<TSource, TDto, TDestination>(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<TSource, TDto>();
			exp.CreateMap<TDto, TDestination>();
		}

		private static void AddDocumentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewDocumentModel, NewDocumentDto>();

			exp.CreateMap<Document, DocumentDetailDto, DocumentDetailModel>();
			exp.CreateMap<UpdateDocumentModel, UpdateDocumentDto, Document>();

			exp.CreateMap<DocumentFilterModel, DocumentFilterDto>();
		}

		private static void AddUserMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewUserModel, User>();

			exp.CreateMap<User, UserIdentityDto>();
			exp.CreateMap<User, UserPreviewDto>()
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId));
			exp.CreateMap<User, EmailRecipientDto>();
			exp.CreateMap<User, UserDetailModel>()
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed))
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId));

			exp.CreateMap<UserPreviewDto, UserPreviewModel>();

			exp.CreateMap<UserProfileModel, UserProfileDto, User>();
			exp.CreateMap<UserFilterModel, UserFilterDto>();

			exp.CreateMap<UserReferenceDto, UserReferenceModel>();
		}

		private static void AddGameMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewGameModel, NewGameDto>();

			exp.CreateMap<Game, GameDetailDto, GameDetailModel>();
			exp.CreateMap<Game, GamePreviewDto, GamePreviewModel>();
			exp.CreateMap<Game, GameReferenceDto, GameReferenceModel>();
			exp.CreateMap<UpdateGameModel, UpdateGameDto, Game>();

			exp.CreateMap<GameFilterModel, GameFilterDto>();

			exp.CreateMap<Game, GameReferenceDto>();
			exp.CreateMap<GameReferenceDto, GameReferenceModel>();
		}

		private static void AddTournamentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewTournamentModel, NewTournamentDto>();

			exp.CreateMap<Tournament, TournamentDetailDto, TournamentDetailModel>();
			exp.CreateMap<Tournament, TournamentPreviewDto, TournamentPreviewModel>();
			exp.CreateMap<Tournament, TournamentReferenceDto, TournamentReferenceModel>();
			exp.CreateMap<UpdateTournamentModel, UpdateTournamentDto, Tournament>();

			exp.CreateMap<TournamentFilterModel, TournamentFilterDto>();

			exp.CreateMap<TournamentParticipantDto, TournamentParticipantPreview>();
		}

		private static void AddSubmissionMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Submission, SubmissionStorageDto>();

			exp.CreateMap<Submission, SubmissionReferenceDto>();
			exp.CreateMap<SubmissionReferenceDto, SubmissionReferenceModel>();

			exp.CreateMap<SubmissionParticipation, SubmissionParticipationDto>();
			exp.CreateMap<SubmissionParticipationDto, SubmissionParticipationModel>();
		}

		private static void AddEmailMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Email, EmailPreviewDto>();
		}

		private static void AddEmailTemplateMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<EmailTemplate, EmailTemplateDto>();
		}

		private static void AddMatchMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Match, MatchDetailDto>();

			exp.CreateMap<MatchDetailDto, MatchDetailModel>();
			exp.CreateMap<MatchFilterModel, MatchFilterDto>();

			exp.CreateMap<Match, MatchReferenceDto>();
			exp.CreateMap<MatchReferenceDto, MatchReferenceModel>();

			exp.CreateMap<MatchExecution, MatchExecutionDto>();
			exp.CreateMap<MatchExecutionDto, MatchExecutionModel>();

			exp.CreateMap<SubmissionMatchResult, SubmissionMatchResultDto>();
			exp.CreateMap<SubmissionMatchResultDto, SubmissionMatchResultModel>();

			exp.CreateMap<MatchExecution, MatchExecutionStorageDto>();
		}
	}
}