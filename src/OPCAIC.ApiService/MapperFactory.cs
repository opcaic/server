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
			exp.CreateMap(typeof(ListDto<>), typeof(ListModel<>), MemberList.Destination);
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

			exp.CreateMap<User, UserPreviewDto>(MemberList.Destination)
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId))
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed));

			exp.CreateMap<User, EmailRecipientDto>(MemberList.Destination);
			exp.CreateMap<User, UserDetailModel>(MemberList.Destination)
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed))
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId));

			exp.CreateMap<UserPreviewDto, UserPreviewModel>(MemberList.Destination);

			exp.CreateMap<UserProfileModel, UserProfileDto, User>(MemberList.Source);
			exp.CreateMap<UserFilterModel, UserFilterDto>(MemberList.Source);

			exp.CreateMap<User, UserReferenceDto, UserReferenceModel>(MemberList.Destination);
		}

		private static void AddGameMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewGameModel, NewGameDto>(MemberList.Source);

			exp.CreateMap<Game, GameDetailDto, GameDetailModel>(MemberList.Destination);
			exp.CreateMap<Game, GamePreviewDto, GamePreviewModel>(MemberList.Destination);
			exp.CreateMap<Game, GameReferenceDto, GameReferenceModel>(MemberList.Destination);
			exp.CreateMap<UpdateGameModel, UpdateGameDto, Game>(MemberList.Source);

			exp.CreateMap<GameFilterModel, GameFilterDto>(MemberList.Source);
		}

		private static void AddTournamentMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewTournamentModel, NewTournamentDto, Tournament>(MemberList.Source);

			exp.CreateMap<Tournament, TournamentAuthDto>(MemberList.Destination)
				.ForMember(d => d.ManagerIds,
					opt => opt.MapFrom(s => s.Managers.Select(m => m.UserId)));

			exp.CreateMap<Tournament, TournamentDetailDto, TournamentDetailModel>(MemberList
				.Destination);
			exp.CreateMap<Tournament, TournamentPreviewDto, TournamentPreviewModel>(MemberList
				.Destination);
			exp.CreateMap<Tournament, TournamentReferenceDto, TournamentReferenceModel>(MemberList
				.Destination);
			exp.CreateMap<UpdateTournamentModel, UpdateTournamentDto, Tournament>(MemberList
				.Source);

			exp.CreateMap<TournamentFilterModel, TournamentFilterDto>(MemberList.Source);

			exp.CreateMap<TournamentParticipantDto, TournamentParticipantPreviewModel>(MemberList
				.Destination);
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
		}

		private static void AddEmailMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Email, EmailPreviewDto>(MemberList.Destination);
		}

		private static void AddEmailTemplateMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<EmailTemplate, EmailTemplateDto>(MemberList.Destination);
		}

		private static void AddMatchMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<MatchFilterModel, MatchFilterDto>(MemberList.Source);

			exp.CreateMap<Match, MatchDetailDto>(MemberList.Destination)
				.ForMember(d => d.State, 
					opt => opt.MapFrom(m => m.State))
				.ForMember(d => d.Submissions,
					opt => opt.MapFrom(m => m.Participations.Select(p => p.Submission)));

			exp.CreateMap<MatchDetailDto, MatchDetailModel>(MemberList.Destination);
			exp.CreateMap<Match, MatchReferenceDto, MatchReferenceModel>(MemberList.Destination);
			exp.CreateMap<MatchExecution, MatchExecutionDto, MatchExecutionModel>(MemberList
				.Destination);
			exp.CreateMap<SubmissionMatchResult, SubmissionMatchResultDto,
				SubmissionMatchResultModel>(MemberList.Destination);

			exp.CreateMap<MatchExecution, MatchExecutionStorageDto>(MemberList.Destination);
		}

		private static void AddBrokerMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<WorkItem, WorkItemDto, WorkItemModel>(MemberList.Destination);
			exp.CreateMap<BrokerStats, BrokerStatsDto, BrokerStatsModel>(MemberList.Destination);
			exp.CreateMap<WorkerInfo, WorkerInfoDto, WorkerInfoModel>(MemberList.Destination);
			exp.CreateMap<WorkMessageBase, WorkMessageBaseDto, WorkMessageBaseModel>(MemberList
				.Destination);
			exp.CreateMap<BotInfo, BotInfoDto, BotInfoModel>(MemberList.Destination);
			exp.CreateMap<MatchExecutionRequest, MatchExecutionRequestDto,
				MatchExecutionRequestModel>(MemberList.Destination);
			exp.CreateMap<SubmissionValidationRequest, SubmissionValidationRequestDto,
				SubmissionValidationRequestModel>(MemberList.Destination);
		}
	}
}