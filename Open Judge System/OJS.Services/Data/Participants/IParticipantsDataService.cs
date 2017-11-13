﻿namespace OJS.Services.Data.Participants
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsDataService : IService
    {
        IQueryable<Participant> GetAll();

        bool Any(int contestId, string userId, bool isOfficial);

        Participant GetWithContest(int contestId, string userId, bool isOfficial);

        IQueryable<Participant> GetWithUsersAndScoresForContest(int contestId, bool isOfficial);

        IQueryable<Participant> GetWithUsersScoresAndSubmissionForContest(int contestId, bool isOfficial);
    }
}