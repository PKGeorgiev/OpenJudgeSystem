﻿namespace OJS.Services.Business.Participants
{
    using System;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;

    public class ParticipantsBusinessService : IParticipantsBusinessService
    {
        private readonly IParticipantsDataService participantsData;
        private readonly IContestsDataService contestsData;

        public ParticipantsBusinessService(
            IParticipantsDataService participantsData,
            IContestsDataService contestsData)
        {
            this.participantsData = participantsData;
            this.contestsData = contestsData;
        }

        public Participant CreateNewByContestByUserByIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin)
        {
            Participant participant;
            if (contest.IsOnline)
            {
                participant = new Participant(contest.Id, userId, isOfficial)
                {
                    ParticipationStartTime = DateTime.Now,
                    ParticipationEndTime = DateTime.Now + contest.Duration
                };

                if (isOfficial &&
                    !isAdmin &&
                    !this.contestsData.IsUserLecturerInByContestAndUser(contest.Id, userId))
                {
                    this.AssignRandomProblemsToParticipant(participant, contest);
                }
            }
            else
            {
                participant = new Participant(contest.Id, userId, isOfficial);
            }

            this.participantsData.Add(participant);
            return participant;
        }

        public void UpdateContestEndTimeForAllParticipantsByContestByParticipantContestStartTimeRangeAndTimeIntervalInMinutes(
            int contestId,
            int minutes,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd)
        {
            var contest = this.contestsData.GetById(contestId);
            var contestTotalDurationInMinutes = contest.Duration.Value.TotalMinutes;

            var participantsInTimeRange =
                this.participantsData.GetAllOfficialInOnlineContestByContestAndContestStartTimeRange(
                    contestId,
                    contestStartTimeRangeStart,
                    contestStartTimeRangeEnd);

            this.participantsData.Update(
                participantsInTimeRange
                    .Where(p => SqlFunctions.DateAdd("minute", minutes, p.ParticipationEndTime) >=
                        SqlFunctions.DateAdd("minute", contestTotalDurationInMinutes, p.ParticipationStartTime)),
                p => new Participant
                {
                    ParticipationEndTime = SqlFunctions.DateAdd(
                    "minute",
                    minutes,
                    p.ParticipationEndTime)
                });
        }

        public IQueryable<Participant> GetAllParticipantsWhoWouldBeReducedBelowDefaultContestDuration(
            int contestId,
            int minutes,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd)
        {
            var contest = this.contestsData.GetById(contestId);
            var contestTotalDurationInMinutes = contest.Duration.Value.TotalMinutes;

            var participantsInvalidForUpdate =
                this.participantsData
                    .GetAllOfficialInOnlineContestByContestAndContestStartTimeRange(
                        contestId,
                        contestStartTimeRangeStart,
                        contestStartTimeRangeEnd)
                    .Where(p => SqlFunctions.DateAdd("minute", minutes, p.ParticipationEndTime) <
                                SqlFunctions.DateAdd("minute", contestTotalDurationInMinutes, p.ParticipationStartTime));            

            return participantsInvalidForUpdate;
        }

        private void AssignRandomProblemsToParticipant(Participant participant, Contest contest)
        {
            var random = new Random();

            foreach (var problemGroup in contest.ProblemGroups)
            {
                var problemsInGroup = problemGroup.Problems.ToList();
                var randomProblem = problemsInGroup[random.Next(0, problemsInGroup.Count)];
                participant.Problems.Add(randomProblem);
            }
        }
    }
}