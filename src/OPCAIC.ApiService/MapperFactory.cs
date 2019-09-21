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
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Broker;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Dtos.Emails;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.SubmissionValidations.Events;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Broker;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Messaging.Messages;
using OPCAIC.Persistence.Repositories;

namespace OPCAIC.ApiService
{
	// TODO: Split to separate profiles and gradually move to Application project
	public class AutomapperProfile : Profile
	{
		public AutomapperProfile()
		{
			AddUserMapping();
			AddTournamentMapping();
			AddTournamentParticipationMapping();
			AddSubmissionMapping();
			AddSubmissionValidationMapping();
			AddDocumentMapping();
			AddMatchMapping();
			AddMatchExecutionMapping();
			AddEmailMapping();
			AddEmailTemplateMapping();
			AddGameMapping();
			AddBrokerMapping();
			AddOther();
		}

		private void CreateMap<TSource, TDto, TDestination>(
			MemberList memberList = MemberList.None)
		{
			CreateMap<TSource, TDto>(memberList);
			CreateMap<TDto, TDestination>(memberList);
		}

		private void AddOther()
		{
			CreateMap(typeof(ListDto<>), typeof(ListModel<>), MemberList.Destination);
			CreateMap(typeof(PagedResult<>), typeof(ListModel<>), MemberList.Destination);

			CreateMap<Dictionary<string, object>, string>()
				.ConvertUsing(d => JsonConvert.SerializeObject(d));
			CreateMap<JObject, string>()
				.ConvertUsing(j => j == null ? null : JsonConvert.SerializeObject(j));
			CreateMap<string, JObject>().ConvertUsing(j => j == null ? null : JObject.Parse(j));
			CreateMap<SubTaskResult, EntryPointResult>()
				.ConvertUsing(s => SubTaskResultToEntryPointResult(s));
			CreateMap<JobStatus, WorkerJobState>()
				.ConvertUsing(s => JobStatusToWorkerJobState(s));
		}

		private WorkerJobState JobStatusToWorkerJobState(JobStatus status)
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

		private EntryPointResult SubTaskResultToEntryPointResult(SubTaskResult status)
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

		private void AddDocumentMapping()
		{
			CreateMap<NewDocumentModel, NewDocumentDto, Document>(MemberList.Source);

			CreateMap<Document, DocumentDetailDto, DocumentDetailModel>(MemberList.Destination);
			CreateMap<UpdateDocumentModel, UpdateDocumentDto, Document>(MemberList.Source);

			CreateMap<DocumentFilterModel, DocumentFilterDto>(MemberList.Destination);
		}

		private void AddUserMapping()
		{
			CreateMap<NewUserModel, User>(MemberList.Source)
				.ForSourceMember(m => m.Password, opt => opt.DoNotValidate());

			CreateMap<User, UserPreviewDto>(MemberList.Destination)
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId))
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed));

			CreateMap<User, EmailRecipientDto>(MemberList.Destination);

			CreateMap<User, UserDetailModel>(MemberList.Destination)
				.ForMember(usr => usr.UserRole,
					opt => opt.MapFrom(usr => usr.RoleId))
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed));

			CreateMap<UserPreviewDto, UserPreviewModel>(MemberList.Destination);

			CreateMap<UserProfileModel, UserProfileDto, User>(MemberList.Source);
			CreateMap<UserFilterModel, UserFilterDto>(MemberList.Source);

			CreateMap<UserReferenceDto, UserLeaderboardViewModel>(MemberList.Destination);
			CreateMap<User, UserReferenceDto, UserReferenceModel>(MemberList.Destination);
		}

		private void AddGameMapping()
		{
			CreateMap<NewGameModel, NewGameDto, Game>(MemberList.Source);

			CreateMap<Game, GameDetailDto>(MemberList.Destination)
				.IncludeBase<Game, GamePreviewModel>();

			CreateMap<GameDetailDto, GameDetailModel>(MemberList.Destination);

			CreateMap<Game, GamePreviewModel>(MemberList.Destination)
				.ForMember(d => d.ActiveTournamentsCount,
					opt => opt.MapFrom(GameRepository.ActiveTournamentsExpression));

			CreateMap<Game, GameReferenceDto, GameReferenceModel>(MemberList.Destination);
			CreateMap<UpdateGameModel, UpdateGameDto, Game>(MemberList.Source);

			CreateMap<GameFilterModel, GameFilterDto>(MemberList.Source);
		}

		private void AddTournamentMapping()
		{
			CreateMap<NewTournamentModel, NewTournamentDto, Tournament>(MemberList.Source);

			CreateMap<Tournament, TournamentAuthDto>(MemberList.Destination)
				.ForMember(d => d.ManagerIds,
					opt => opt.MapFrom(s => s.Managers.Select(m => m.UserId)))
				.ForMember(d => d.ParticipantIds,
					opt => opt.MapFrom(e => e.Participants.Select(s => s.UserId)));

			CreateMap<Tournament, TournamentDetailDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentPreviewDto>();

			CreateMap<TournamentDetailDto, TournamentDetailModel>(MemberList
				.Destination);

			CreateMap<Tournament, TournamentDtoBase>(MemberList
					.Destination)
				.ForMember(d => d.ActiveSubmissionsCount,
					opt => opt.MapFrom(s
						=> s.Participants.Count(e => e.ActiveSubmissionId != null)))
				.ForMember(d => d.PlayersCount,
					opt => opt.MapFrom(s
						=> s.Participants.Count))
				.ForMember(d => d.ImageUrl,
					opt => opt.MapFrom(s
						=> s.ImageUrl ?? s.Game.DefaultTournamentImageUrl))
				.ForMember(d => d.ImageOverlay,
					opt => opt.MapFrom(s
						=> s.ImageOverlay ?? s.Game.DefaultTournamentImageOverlay))
				.ForMember(d => d.ThemeColor,
					opt => opt.MapFrom(s
						=> s.ThemeColor ?? s.Game.DefaultTournamentThemeColor))
				.ForMember(d => d.SubmissionsCount,
					opt => opt.MapFrom(s => s.Participants.Sum(p => p.Submissions.Count)));

			CreateMap<Tournament, TournamentPreviewDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentDtoBase>()
				.ForMember(t => t.LastUserSubmissionDate, opt => opt.Ignore());

			CreateMap<Tournament, TournamentStateInfoDto>(MemberList.Destination);
			CreateMap<Tournament, TournamentReferenceDto, TournamentReferenceModel>(MemberList
				.Destination);
			CreateMap<UpdateTournamentModel, UpdateTournamentDto, Tournament>(MemberList
				.Source);

			CreateMap<TournamentFilterModel, TournamentFilterDto>(MemberList.Source);

			CreateMap<TournamentInvitationDto, TournamentInvitationPreviewModel>(MemberList
				.Destination);
			CreateMap<TournamentInvitationFilter, TournamentInvitationFilterDto>(MemberList
				.Destination);
			CreateMap<TournamentDetailDto, TournamentReferenceModel>(MemberList.Destination);

			CreateMap<Tournament, TournamentGenerationDtoBase>(MemberList.Destination)
				.ForMember(d => d.ActiveSubmissionIds,
					opt => opt.MapFrom(f
						=> f.Participants.Where(s => s.ActiveSubmissionId != null)
							.Select(s => s.ActiveSubmissionId.Value)));

			CreateMap<Tournament, TournamentBracketsGenerationDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentGenerationDtoBase>();

			CreateMap<Tournament, TournamentOngoingGenerationDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentGenerationDtoBase>()
				.ForMember(t => t.Submissions,
					opt => opt.MapFrom(x => x.Participants.SelectMany(p => p.Submissions)));

			CreateMap<Tournament, TournamentDeadlineGenerationDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentGenerationDtoBase>();

			CreateMap<TournamentStateUpdateDto, Tournament>(MemberList.Source);
			CreateMap<TournamentFinishedUpdateDto, Tournament>(MemberList.Source);
			CreateMap<TournamentStartedUpdateDto, Tournament>(MemberList.Source);
		}

		private void AddTournamentParticipationMapping()
		{
			CreateMap<UpdateTournamentParticipationDto, TournamentParticipation>(MemberList
				.Source);
		}

		private void AddSubmissionMapping()
		{
			CreateMap<NewSubmissionModel, NewSubmissionDto>(MemberList.Source)
				.ForSourceMember(d => d.Archive, opt => opt.DoNotValidate());
			CreateMap<NewSubmissionDto, Submission>(MemberList.Source);

			CreateMap<Submission, SubmissionAuthDto>(MemberList.Destination)
				.ForMember(d => d.TournamentManagersIds,
					opt => opt.MapFrom(s => s.Tournament.Managers.Select(m => m.UserId)));

			CreateMap<Submission, SubmissionPreviewDto>(MemberList.Destination)
				.ForMember(s => s.LastValidation, opt => opt.MapFrom(
					s => s.Validations.OrderByDescending(v => v.Id).First()))
				.ForMember(s => s.IsActive,
					opt => opt.MapFrom(s => s.TournamentParticipation.ActiveSubmissionId == s.Id));

			CreateMap<Submission, SubmissionDetailDto>(MemberList.Destination)
				.IncludeBase<Submission, SubmissionPreviewDto>();

			CreateMap<SubmissionPreviewDto, SubmissionPreviewModel>(MemberList.Destination)
				.ForMember(s => s.ValidationState, opt => opt.Ignore());

			CreateMap<SubmissionDetailDto, SubmissionDetailModel>(MemberList.Destination)
				.IncludeBase<SubmissionPreviewDto, SubmissionPreviewModel>();

			CreateMap<Submission, SubmissionReferenceDto, SubmissionReferenceModel>(MemberList
				.Destination);
			CreateMap<Submission, SubmissionScoreViewDto>(MemberList.Destination);

			CreateMap<SubmissionFilterModel, SubmissionFilterDto>(MemberList.Destination);

			CreateMap<Submission, SubmissionStorageDto>(MemberList.Destination);
			CreateMap<Submission, UpdateSubmissionScoreDto>(MemberList.Destination);
			CreateMap<UpdateSubmissionScoreDto, Submission>(MemberList.Source);

			CreateMap<SubmissionDetailDto, LeaderboardParticipationModel>(MemberList.None)
				.ForMember(s => s.User, opt => opt.MapFrom(s => s.Author));
		}

		private void AddSubmissionValidationMapping()
		{
			CreateMap<SubmissionValidation, SubmissionValidationStorageDto>(MemberList
				.Destination);
			CreateMap<SubmissionValidationDto, SubmissionValidationStorageDto>(MemberList
				.Destination);

			CreateMap<NewSubmissionValidationDto, SubmissionValidation>(MemberList.Source);
			CreateMap<SubmissionValidationResult, SubmissionValidationFinished>(MemberList
					.Destination)
				.ForMember(d => d.State, opt => opt.MapFrom(r => r.JobStatus))
				.ForMember(d => d.Executed, opt => opt.Ignore());
			CreateMap<SubmissionValidationFinished, SubmissionValidation>(MemberList.Source);
			CreateMap<JobStateUpdateDto, SubmissionValidation>(MemberList.Source);
			CreateMap<SubmissionValidation, SubmissionValidationRequestDataDto>(MemberList
					.Destination)
				.ForMember(v => v.TournamentId, opt => opt.MapFrom(v => v.Submission.Tournament.Id))
				.ForMember(v => v.TournamentConfiguration,
					opt => opt.MapFrom(v => v.Submission.Tournament.Configuration))
				.ForMember(v => v.GameKey,
					opt => opt.MapFrom(v => v.Submission.Tournament.Game.Key));

			CreateMap<SubmissionValidation, SubmissionValidationDto>(MemberList
				.Destination);
			CreateMap<SubmissionValidationDto, SubmissionValidationPreviewModel>(
				MemberList.Destination);

			CreateMap<SubmissionValidationDto, SubmissionValidationDetailModel>(
					MemberList.Destination)
				.ForMember(d => d.CheckerLog, opt => opt.Ignore())
				.ForMember(d => d.CompilerLog, opt => opt.Ignore())
				.ForMember(d => d.ValidatorLog, opt => opt.Ignore());

			CreateMap<SubmissionValidationLogsDto, SubmissionValidationDetailModel>(MemberList
				.Source);

			CreateMap<SubmissionValidation, SubmissionValidationAuthDto>(MemberList.Destination)
				.ForMember(v => v.TournamentOwnerId,
					opt => opt.MapFrom(v => v.Submission.Tournament.OwnerId))
				.ForMember(v => v.TournamentManagersIds,
					opt => opt.MapFrom(v
						=> v.Submission.Tournament.Managers.Select(m => m.UserId)));
		}

		private void AddEmailMapping()
		{
			CreateMap<Email, EmailPreviewDto>(MemberList.Destination);
			CreateMap<NewEmailDto, Email>(MemberList.Source);
		}

		private void AddEmailTemplateMapping()
		{
			CreateMap<EmailTemplate, EmailTemplateDto>(MemberList.Destination);
		}

		private void AddMatchMapping()
		{
			CreateMap<NewMatchDto, Match>(MemberList.Source)
				.ForSourceMember(m => m.Submissions,
					opt => opt.DoNotValidate());

			CreateMap<Match, MatchReferenceDto, MatchReferenceModel>(MemberList.Destination);
		}

		private void AddMatchExecutionMapping()
		{
			CreateMap<NewMatchExecutionDto, MatchExecution>(MemberList.Source);
			CreateMap<MatchExecution, MatchExecutionAuthDto>(MemberList.Destination)
				.ForMember(d => d.TournamentManagersIds,
					opt => opt.MapFrom(e => e.Match.Tournament.Managers.Select(m => m.UserId)))
				.ForMember(d => d.TournamentOwnerId,
					opt => opt.MapFrom(e => e.Match.Tournament.OwnerId))
				.ForMember(d => d.MatchParticipantsUserIds,
					opt => opt.MapFrom(e
						=> e.Match.Participations.Select(p => p.Submission.AuthorId)));

			CreateMap<MatchExecution, MatchExecutionStorageDto>(MemberList.Destination);
			CreateMap<MatchExecutionDto, MatchExecutionStorageDto>(MemberList.Destination);

			CreateMap<MatchExecution, MatchExecutionDto, MatchExecutionPreviewModel>(MemberList
				.Destination);
			CreateMap<MatchExecutionDto, MatchExecutionDetailModel>(MemberList.Destination)
				.ForMember(e => e.ExecutorLog, opt => opt.Ignore());
			CreateMap<SubmissionMatchResult, SubmissionMatchResultDto,
				SubmissionMatchResultPreviewModel>(MemberList.Destination);
			CreateMap<SubmissionMatchResultPreviewModel, SubmissionMatchResultDetailModel>(
				MemberList.Source);

			CreateMap<MatchExecutionResult, UpdateMatchExecutionDto>()
				.ForMember(d => d.State, opt => opt.MapFrom(r => r.JobStatus))
				.ForMember(d => d.Executed, opt => opt.Ignore());
			CreateMap<BotResult, NewSubmissionMatchResultDto>(MemberList.Source);

			CreateMap<NewSubmissionMatchResultDto, SubmissionMatchResult>(MemberList.Source);
			CreateMap<UpdateMatchExecutionDto, MatchExecution>(MemberList.Source);
			CreateMap<JobStateUpdateDto, MatchExecution>(MemberList.Source);
			CreateMap<MatchExecution, MatchExecutionRequestDataDto>(MemberList
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

		private void AddBrokerMapping()
		{
			CreateMap<WorkItem, WorkItemDto, WorkItemModel>(MemberList.Destination);
			CreateMap<BrokerStats, BrokerStatsModel>(MemberList.Destination);
			CreateMap<WorkerInfo, WorkerInfoModel>(MemberList.Destination);
			CreateMap<WorkMessageBase, WorkMessageBaseDto, WorkMessageBaseModel>(MemberList
				.Destination);
			CreateMap<BotInfo, BotInfoDto, BotInfoModel>(MemberList.Destination);
			CreateMap<MatchExecutionRequest, MatchExecutionRequestDto,
				MatchExecutionRequestModel>(MemberList.Destination);
			CreateMap<SubmissionValidationRequest, SubmissionValidationRequestDto,
				SubmissionValidationRequestModel>(MemberList.Destination);
		}
	}
}