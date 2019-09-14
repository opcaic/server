using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Models.Leaderboards;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.SubmissionValidations;
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
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
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
				exp.AddSubmissionValidationMapping();
				exp.AddDocumentMapping();
				exp.AddMatchMapping();
				exp.AddMatchExecutionMapping();
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

			exp.CreateMap<Dictionary<string, object>, string>()
				.ConvertUsing(d => JsonConvert.SerializeObject(d));
			exp.CreateMap<JObject, string>().ConvertUsing(j => JsonConvert.SerializeObject(j));
			exp.CreateMap<string, JObject>().ConvertUsing(j => j == null ? null : JObject.Parse(j));
			exp.CreateMap<SubTaskResult, EntryPointResult>()
				.ConvertUsing(s => SubTaskResultToEntryPointResult(s));
			exp.CreateMap<JobStatus, WorkerJobState>()
				.ConvertUsing(s => JobStatusToWorkerJobState(s));
		}

		private static WorkerJobState JobStatusToWorkerJobState(JobStatus status)
		{
			switch (status)
			{
				case JobStatus.Ok:
				case JobStatus.Timeout:
				case JobStatus.Error:
					return WorkerJobState.Finished;

				case JobStatus.Canceled:
					return WorkerJobState.Cancelled;

				case JobStatus.Unknown:
				default:
					throw new ArgumentOutOfRangeException(nameof(status), status, null);
			}
		}

		private static EntryPointResult SubTaskResultToEntryPointResult(SubTaskResult status)
		{
			switch (status)
			{
				case SubTaskResult.Unknown:
					return EntryPointResult.NotExecuted;
				case SubTaskResult.Ok:
					return EntryPointResult.Success;
				case SubTaskResult.NotOk:
					return EntryPointResult.UserError;
				case SubTaskResult.Aborted:
					return EntryPointResult.Cancelled;
				case SubTaskResult.ModuleError:
					return EntryPointResult.ModuleError;
				case SubTaskResult.PlatformError:
					return EntryPointResult.PlatformError;
				default:
					throw new ArgumentOutOfRangeException(nameof(status), status, null);
			}
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
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId))
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed));

			exp.CreateMap<UserPreviewDto, UserPreviewModel>(MemberList.Destination);

			exp.CreateMap<UserProfileModel, UserProfileDto, User>(MemberList.Source);
			exp.CreateMap<UserFilterModel, UserFilterDto>(MemberList.Source);

			exp.CreateMap<UserReferenceDto, UserLeaderboardViewModel>(MemberList.Destination);
			exp.CreateMap<User, UserReferenceDto, UserReferenceModel>(MemberList.Destination);
		}

		private static void AddGameMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewGameModel, NewGameDto, Game>(MemberList.Source);

			exp.CreateMap<Game, GameDetailDto>(MemberList.Destination)
				.IncludeBase<Game, GamePreviewDto>();

			exp.CreateMap<GameDetailDto, GameDetailModel>(MemberList.Destination);

			exp.CreateMap<Game, GamePreviewDto>(MemberList.Destination)
				.ForMember(d => d.ActiveTournamentsCount,
					opt => opt.MapFrom(GameRepository.ActiveTournamentsExpression));

			exp.CreateMap<GamePreviewDto, GamePreviewModel>(MemberList.Destination);

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

			exp.CreateMap<Tournament, TournamentDetailDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentPreviewDto>();

			exp.CreateMap<TournamentDetailDto, TournamentDetailModel>(MemberList
				.Destination);

			exp.CreateMap<Tournament, TournamentPreviewDto>(MemberList
					.Destination)
				.ForMember(d => d.ActiveSubmissionsCount,
					opt => opt.MapFrom(s => s.Submissions.Count(e => e.IsActive)))
				.ForMember(d => d.PlayersCount,
					opt => opt.MapFrom(s
						=> s.Submissions.Select(x => x.Author.Id).Distinct().Count()))
				.ForMember(d => d.ImageUrl,
					opt => opt.MapFrom(s
						=> s.ImageUrl ?? s.Game.DefaultTournamentImage))
				.ForMember(d => d.ImageOverlay,
					opt => opt.MapFrom(s
						=> s.ImageOverlay ?? s.Game.DefaultTournamentImageOverlay))
				.ForMember(d => d.ThemeColor,
					opt => opt.MapFrom(s
						=> s.ThemeColor ?? s.Game.DefaultTournamentThemeColor));

			exp.CreateMap<TournamentPreviewDto, TournamentPreviewModel>(MemberList
				.Destination);

			exp.CreateMap<Tournament, TournamentStateInfoDto>(MemberList.Destination);
			exp.CreateMap<Tournament, TournamentReferenceDto, TournamentReferenceModel>(MemberList
				.Destination);
			exp.CreateMap<UpdateTournamentModel, UpdateTournamentDto, Tournament>(MemberList
				.Source);

			exp.CreateMap<TournamentFilterModel, TournamentFilterDto>(MemberList.Source);

			exp.CreateMap<TournamentParticipantDto, TournamentParticipantPreviewModel>(MemberList
				.Destination);
			exp.CreateMap<TournamentParticipantFilter, TournamentParticipantFilterDto>(MemberList
				.Destination);
			exp.CreateMap<TournamentDetailDto, TournamentReferenceModel>(MemberList.Destination);

			exp.CreateMap<Tournament, TournamentGenerationDtoBase>(MemberList.Destination)
				.ForMember(d => d.ActiveSubmissionIds,
					opt => opt.MapFrom(f
						=> f.Submissions.Where(s => s.IsActive).Select(s => s.Id)));

			exp.CreateMap<Tournament, TournamentBracketsGenerationDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentGenerationDtoBase>();

			exp.CreateMap<Tournament, TournamentOngoingGenerationDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentGenerationDtoBase>();

			exp.CreateMap<Tournament, TournamentDeadlineGenerationDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentGenerationDtoBase>();

			exp.CreateMap<TournamentStateUpdateDto, Tournament>(MemberList.Source);
			exp.CreateMap<TournamentFinishedUpdateDto, Tournament>(MemberList.Source);
			exp.CreateMap<TournamentStartedUpdateDto, Tournament>(MemberList.Source);
		}

		private static void AddSubmissionMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewSubmissionModel, NewSubmissionDto>(MemberList.Source)
				.ForSourceMember(d => d.Archive, opt => opt.DoNotValidate());
			exp.CreateMap<NewSubmissionDto, Submission>(MemberList.Source);

			exp.CreateMap<Submission, SubmissionAuthDto>(MemberList.Destination)
				.ForMember(d => d.TournamentManagersIds,
					opt => opt.MapFrom(s => s.Tournament.Managers.Select(m => m.UserId)));

			exp.CreateMap<Submission, SubmissionPreviewDto>(MemberList.Destination)
				.ForMember(s => s.LastValidation, opt => opt.MapFrom(
					s => s.Validations.OrderByDescending(v => v.Id).First()));

			exp.CreateMap<Submission, SubmissionDetailDto>(MemberList.Destination)
				.IncludeBase<Submission, SubmissionPreviewDto>();

			exp.CreateMap<SubmissionPreviewDto, SubmissionPreviewModel>(MemberList.Destination)
				.ForMember(s => s.ValidationState, opt => opt.Ignore());

			exp.CreateMap<SubmissionDetailDto, SubmissionDetailModel>(MemberList.Destination)
				.IncludeBase<SubmissionPreviewDto, SubmissionPreviewModel>();

			exp.CreateMap<Submission, SubmissionReferenceDto, SubmissionReferenceModel>(MemberList
				.Destination);

			exp.CreateMap<UpdateSubmissionModel, UpdateSubmissionDto, Submission>(MemberList
				.Source);
			exp.CreateMap<SubmissionFilterModel, SubmissionFilterDto>(MemberList.Destination);

			exp.CreateMap<Submission, SubmissionStorageDto>(MemberList.Destination);
			exp.CreateMap<Submission, UpdateSubmissionScoreDto>(MemberList.Destination);
			exp.CreateMap<SubmissionDetailDto, LeaderboardParticipationModel>(MemberList.None)
				.ForMember(s => s.User, opt => opt.MapFrom(s => s.Author));
		}

		private static void AddSubmissionValidationMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<SubmissionValidation, SubmissionValidationStorageDto>(MemberList
				.Destination);
			exp.CreateMap<SubmissionValidationDto, SubmissionValidationStorageDto>(MemberList
				.Destination);

			exp.CreateMap<NewSubmissionValidationDto, SubmissionValidation>(MemberList.Source);
			exp.CreateMap<SubmissionValidationResult, UpdateSubmissionValidationDto>(MemberList
					.Destination)
				.ForMember(d => d.State, opt => opt.MapFrom(r => r.JobStatus))
				.ForMember(d => d.Executed, opt => opt.Ignore());
			exp.CreateMap<UpdateSubmissionValidationDto, SubmissionValidation>(MemberList.Source);
			exp.CreateMap<JobStateUpdateDto, SubmissionValidation>(MemberList.Source);
			exp.CreateMap<SubmissionValidation, SubmissionValidationRequestDataDto>(MemberList
					.Destination)
				.ForMember(v => v.TournamentId, opt => opt.MapFrom(v => v.Submission.Tournament.Id))
				.ForMember(v => v.TournamentConfiguration,
					opt => opt.MapFrom(v => v.Submission.Tournament.Configuration))
				.ForMember(v => v.GameKey,
					opt => opt.MapFrom(v => v.Submission.Tournament.Game.Key));

			exp.CreateMap<SubmissionValidation, SubmissionValidationDto>(MemberList
				.Destination);
			exp.CreateMap<SubmissionValidationDto, SubmissionValidationPreviewModel>(
				MemberList.Destination);

			exp.CreateMap<SubmissionValidationDto, SubmissionValidationDetailModel>(
					MemberList.Destination)
				.ForMember(d => d.CheckerLog, opt => opt.Ignore())
				.ForMember(d => d.CompilerLog, opt => opt.Ignore())
				.ForMember(d => d.ValidatorLog, opt => opt.Ignore());

			exp.CreateMap<SubmissionValidationLogsDto, SubmissionValidationDetailModel>(MemberList
				.Source);

			exp.CreateMap<SubmissionValidation, SubmissionValidationAuthDto>(MemberList.Destination)
				.ForMember(v => v.TournamentOwnerId,
					opt => opt.MapFrom(v => v.Submission.Tournament.OwnerId))
				.ForMember(v => v.TournamentManagersIds,
					opt => opt.MapFrom(v
						=> v.Submission.Tournament.Managers.Select(m => m.UserId)));
		}

		private static void AddEmailMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<Email, EmailPreviewDto>(MemberList.Destination);
			exp.CreateMap<NewEmailDto, Email>(MemberList.Source);
		}

		private static void AddEmailTemplateMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<EmailTemplate, EmailTemplateDto>(MemberList.Destination);
		}

		private static void AddMatchMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewMatchDto, Match>(MemberList.Source)
				.ForSourceMember(m => m.Submissions,
					opt => opt.DoNotValidate());

			exp.CreateMap<MatchFilterModel, MatchFilterDto>(MemberList.Source);

			exp.CreateMap<Match, MatchDetailDto>(MemberList.Destination)
				.ForMember(d => d.Submissions,
					opt => opt.MapFrom(m => m.Participations.Select(p => p.Submission)));

			exp.CreateMap<MatchDetailDto, MatchDetailModel>(MemberList.Destination);
			exp.CreateMap<Match, MatchReferenceDto, MatchReferenceModel>(MemberList.Destination);
		}

		private static void AddMatchExecutionMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<NewMatchExecutionDto, MatchExecution>(MemberList.Source);
			exp.CreateMap<MatchExecution, MatchExecutionAuthDto>(MemberList.Destination)
				.ForMember(d => d.TournamentManagersIds,
					opt => opt.MapFrom(e => e.Match.Tournament.Managers.Select(m => m.UserId)))
				.ForMember(d => d.TournamentOwnerId,
					opt => opt.MapFrom(e => e.Match.Tournament.OwnerId))
				.ForMember(d => d.MatchParticipantsUserIds,
					opt => opt.MapFrom(e
						=> e.Match.Participations.Select(p => p.Submission.AuthorId)));

			exp.CreateMap<MatchExecution, MatchExecutionStorageDto>(MemberList.Destination);
			exp.CreateMap<MatchExecutionDto, MatchExecutionStorageDto>(MemberList.Destination);

			exp.CreateMap<MatchExecution, MatchExecutionDto, MatchExecutionPreviewModel>(MemberList
				.Destination);
			exp.CreateMap<MatchExecutionDto, MatchExecutionDetailModel>(MemberList.Destination)
				.ForMember(e => e.ExecutorLog, opt => opt.Ignore());
			exp.CreateMap<SubmissionMatchResult, SubmissionMatchResultDto,
				SubmissionMatchResultPreviewModel>(MemberList.Destination);
			exp.CreateMap<SubmissionMatchResultPreviewModel, SubmissionMatchResultDetailModel>(
				MemberList.Source);

			exp.CreateMap<MatchExecutionResult, UpdateMatchExecutionDto>()
				.ForMember(d => d.State, opt => opt.MapFrom(r => r.JobStatus))
				.ForMember(d => d.Executed, opt => opt.Ignore());
			exp.CreateMap<BotResult, NewSubmissionMatchResultDto>(MemberList.Source);

			exp.CreateMap<NewSubmissionMatchResultDto, SubmissionMatchResult>(MemberList.Source);
			exp.CreateMap<UpdateMatchExecutionDto, MatchExecution>(MemberList.Source);
			exp.CreateMap<JobStateUpdateDto, MatchExecution>(MemberList.Source);
			exp.CreateMap<MatchExecution, MatchExecutionRequestDataDto>(MemberList
					.Destination)
				.ForMember(e => e.SubmissionIds,
					opt => opt.MapFrom(e
						=> e.Match.Participations.OrderBy(s => s.Order)
							.Select(s => s.SubmissionId)))
				.ForMember(e => e.TournamentId, opt => opt.MapFrom(e => e.Match.Tournament.Id))
				.ForMember(e => e.TournamentConfiguration,
					opt => opt.MapFrom(e => e.Match.Tournament.Configuration))
				.ForMember(e => e.GameKey, opt => opt.MapFrom(e => e.Match.Tournament.Game.Key));
		}

		private static void AddBrokerMapping(this IMapperConfigurationExpression exp)
		{
			exp.CreateMap<WorkItem, WorkItemDto, WorkItemModel>(MemberList.Destination);
			exp.CreateMap<BrokerStats, BrokerStatsModel>(MemberList.Destination);
			exp.CreateMap<WorkerInfo, WorkerInfoModel>(MemberList.Destination);
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