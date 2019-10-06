using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.SubmissionValidations;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Broker;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.SubmissionValidations.Events;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Broker;
using OPCAIC.Domain.Entities;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService
{
	public class DynamicProfile : AutoMapperProfile
	{
		public DynamicProfile()
			: base(Assembly.GetExecutingAssembly())
		{
		}
	}

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
			CreateMap<Dictionary<string, object>, string>()
				.ConvertUsing(d => JsonConvert.SerializeObject(d));
			CreateMap<JObject, string>()
				.ConvertUsing(j => j == null ? null : JsonConvert.SerializeObject(j));
			CreateMap<string, JObject>().ConvertUsing(j => j == null ? null : JObject.Parse(j));
		}

		private void AddDocumentMapping()
		{
			CreateMap<NewDocumentModel, NewDocumentDto, Document>(MemberList.Source);
			CreateMap<UpdateDocumentModel, UpdateDocumentDto, Document>(MemberList.Source);
		}

		private void AddUserMapping()
		{
			CreateMap<UserProfileModel, UserProfileDto, User>(MemberList.Source);
			CreateMap<User, UserReferenceDto, UserReferenceModel>(MemberList.Destination);
		}

		private void AddTournamentMapping()
		{
			CreateMap<Tournament, TournamentReferenceDto, TournamentReferenceModel>(MemberList
				.Destination);
			CreateMap<TournamentDetailDto, TournamentReferenceModel>(MemberList.Destination);
			CreateMap<Tournament, Tournament>(MemberList.Destination);
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

			CreateMap<SubmissionDetailDto, SubmissionDetailModel>(MemberList.Destination);

			CreateMap<Submission, SubmissionReferenceDto, SubmissionReferenceModel>(MemberList
				.Destination);
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
				.IgnoreProperty(m => m.Executed);
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