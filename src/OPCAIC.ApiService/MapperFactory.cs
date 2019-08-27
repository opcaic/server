using System.Linq;
using AutoMapper;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Broker;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Dtos.Emails;
using OPCAIC.Infrastructure.Dtos.EmailTemplates;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Messaging.Messages;

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
				exp.AddBrokerMapping();
				exp.AddOther();
			});
		}

		private static void CreateMap<TSource, TDto, TDestination>(
			this IMapperConfigurationExpression exp, MemberList memberList = MemberList.None)
		{
			exp.CreateMap<TSource, TDto>(memberList);
			exp.CreateMap<TDto, TDestination>(memberList);
		}

		private static void AddOther(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap(typeof(ListDto<>), typeof(ListModel<>));
		}

		private static void AddDocumentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewDocumentModel, NewDocumentDto, Document>(MemberList.Source);

			exp.CreateMap<Document, DocumentDetailDto, DocumentDetailModel>(MemberList.Destination);
			exp.CreateMap<UpdateDocumentModel, UpdateDocumentDto, Document>(MemberList.Source);

			exp.CreateMap<DocumentFilterModel, DocumentFilterDto>(MemberList.Destination);
		}

		private static void AddUserMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewUserModel, User>(MemberList.Source)
				.ForSourceMember(m => m.Password, opt => opt.DoNotValidate());

			exp.CreateMap<User, UserPreviewDto>()
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId))
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed));

			exp.CreateMap<User, EmailRecipientDto>();
			exp.CreateMap<User, UserDetailModel>()
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed))
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId));

			exp.CreateMap<UserPreviewDto, UserPreviewModel>();

			exp.CreateMap<UserProfileModel, UserProfileDto, User>();
			exp.CreateMap<UserFilterModel, UserFilterDto>();

			exp.CreateMap<User, UserReferenceDto>();
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
		}

		private static void AddTournamentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewTournamentModel, NewTournamentDto>();

			exp.CreateMap<Tournament, TournamentAuthDto>(MemberList.Destination)
				.ForMember(d => d.ManagerIds,
					opt => opt.MapFrom(s => s.Managers.Select(m => m.UserId)));

			exp.CreateMap<Tournament, TournamentDetailDto, TournamentDetailModel>();
			exp.CreateMap<Tournament, TournamentPreviewDto, TournamentPreviewModel>();
			exp.CreateMap<Tournament, TournamentReferenceDto, TournamentReferenceModel>();
			exp.CreateMap<UpdateTournamentModel, UpdateTournamentDto, Tournament>();

			exp.CreateMap<TournamentFilterModel, TournamentFilterDto>();

			exp.CreateMap<TournamentParticipantDto, TournamentParticipantPreview>();
		}

		private static void AddSubmissionMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewSubmissionModel, NewSubmissionDto>(MemberList.Source)
				.ForSourceMember(d => d.Archive, opt => opt.DoNotValidate());
			exp.CreateMap<NewSubmissionDto, Submission>(MemberList.Source);

			exp.CreateMap<Submission, SubmissionPreviewDto, SubmissionPreviewModel>(MemberList
				.Destination);
			exp.CreateMap<Submission, SubmissionDetailDto, SubmissionDetailModel>(MemberList
				.Destination);
			exp.CreateMap<Submission, SubmissionReferenceDto, SubmissionReferenceModel>(MemberList
				.Destination);

			exp.CreateMap<UpdateSubmissionModel, UpdateSubmissionDto, Submission>(MemberList
				.Source);
			exp.CreateMap<SubmissionFilterModel, SubmissionFilterDto>(MemberList.Destination);

			exp.CreateMap<Submission, SubmissionStorageDto>(MemberList.Destination);

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
			exp.CreateMap<MatchFilterModel, MatchFilterDto>();

			exp.CreateMap<Match, MatchDetailDto, MatchDetailModel>();
			exp.CreateMap<Match, MatchReferenceDto, MatchReferenceModel>();
			exp.CreateMap<MatchExecution, MatchExecutionDto, MatchExecutionModel>();
			exp.CreateMap<SubmissionMatchResult, SubmissionMatchResultDto,
				SubmissionMatchResultModel>();

			exp.CreateMap<MatchExecution, MatchExecutionStorageDto>();
		}

		private static void AddBrokerMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<WorkItem, WorkItemDto, WorkItemModel>();
			exp.CreateMap<BrokerStats, BrokerStatsDto, BrokerStatsModel>();
			exp.CreateMap<WorkerInfo, WorkerInfoDto, WorkerInfoModel>();
			exp.CreateMap<WorkMessageBase, WorkMessageBaseDto, WorkMessageBaseModel>();
			exp.CreateMap<BotInfo, BotInfoDto, BotInfoModel>();
			exp.CreateMap<MatchExecutionRequest, MatchExecutionRequestDto, MatchExecutionRequestModel>();
			exp.CreateMap<SubmissionValidationRequest, SubmissionValidationRequestDto, SubmissionValidationRequestModel>();
		}
	}
}