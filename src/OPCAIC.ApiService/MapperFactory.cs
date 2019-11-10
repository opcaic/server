using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Broker;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.MatchExecutions.Events;
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

		private void AddUserMapping()
		{
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

			CreateMap<Submission, SubmissionReferenceDto, SubmissionReferenceModel>(MemberList
				.Destination);
		}

		private void AddSubmissionValidationMapping()
		{
			CreateMap<NewSubmissionValidationDto, SubmissionValidation>(MemberList.Source);
			CreateMap<SubmissionValidationResult, SubmissionValidationFinished>(MemberList
					.Source)
				.ForMember(d => d.State, opt => opt.MapFrom(r => r.JobStatus))
				.IgnoreProperty(m => m.Executed);

			CreateMap<SubmissionValidationFinished, SubmissionValidation>(MemberList.Source)
				.IgnoreSourceProperty(e => e.TournamentId)
				.IgnoreSourceProperty(e => e.SubmissionId)
				.IgnoreSourceProperty(e => e.ValidationId)
				.IgnoreSourceProperty(e => e.GameId);

			CreateMap<JobStateUpdateDto, SubmissionValidation>(MemberList.Source);
			CreateMap<SubmissionValidation, SubmissionValidationRequestDataDto>(MemberList
					.Destination)
				.ForMember(v => v.GameId, opt => opt.MapFrom(v => v.Submission.Tournament.GameId))
				.ForMember(v => v.TournamentId, opt => opt.MapFrom(v => v.Submission.TournamentId))
				.ForMember(v => v.TournamentConfiguration,
					opt => opt.MapFrom(v => v.Submission.Tournament.Configuration))
				.ForMember(v => v.GameKey,
					opt => opt.MapFrom(v => v.Submission.Tournament.Game.Key));
		}

		private void AddMatchMapping()
		{
			CreateMap<NewMatchDto, Match>(MemberList.Source)
				.ForSourceMember(m => m.Submissions,
					opt => opt.DoNotValidate());

			CreateMap<Match, MatchReferenceDto>(MemberList.Destination);
		}

		private void AddMatchExecutionMapping()
		{
			CreateMap<NewMatchExecutionDto, MatchExecution>(MemberList.Source);

			CreateMap<MatchExecutionResult, MatchExecutionFinished>(MemberList.Source);
			CreateMap<BotResult, SubmissionMatchResult>(MemberList.Source);
			CreateMap<MatchExecutionFinished, MatchExecution>(MemberList.Source)
				.ForMember(d => d.State, opt => opt.MapFrom(r => r.JobStatus))
				.IgnoreSourceProperty(m => m.ExecutionId)
				.IgnoreSourceProperty(m => m.GameId)
				.IgnoreSourceProperty(m => m.MatchId)
				.IgnoreSourceProperty(m => m.TournamentId);

			CreateMap<JobStateUpdateDto, MatchExecution>(MemberList.Source);
			CreateMap<MatchExecution, MatchExecutionRequestDataDto>(MemberList
					.Destination)
				.ForMember(e => e.SubmissionIds,
					opt => opt.MapFrom(e
						=> e.Match.Participations.OrderBy(s => s.Order)
							.Select(s => s.SubmissionId)))
				.ForMember(e => e.GameId, opt => opt.MapFrom(e => e.Match.Tournament.GameId))
				.ForMember(e => e.TournamentId, opt => opt.MapFrom(e => e.Match.Tournament.Id))
				.ForMember(e => e.TournamentConfiguration,
					opt => opt.MapFrom(e => e.Match.Tournament.Configuration))
				.ForMember(e => e.GameKey, opt => opt.MapFrom(e => e.Match.Tournament.Game.Key));
		}

		private void AddBrokerMapping()
		{
			CreateMap<WorkItem, WorkItemDto, WorkItemDto>(MemberList.Destination);
			CreateMap<BrokerStats, BrokerStatsModel>(MemberList.Destination)
				.ForMember(b => b.List, opt => opt.MapFrom(b => b.Workers));
			CreateMap<WorkerInfo, WorkerInfoModel>(MemberList.Destination);
			CreateMap<WorkMessageBase, WorkMessageBaseDto>(MemberList
				.Destination);
			CreateMap<BotInfo, BotInfoDto, BotInfoModel>(MemberList.Destination);
			CreateMap<MatchExecutionRequest, MatchExecutionRequestDto>(MemberList.Destination);
			CreateMap<SubmissionValidationRequest, SubmissionValidationRequestDto>(MemberList.Destination);
		}
	}
}