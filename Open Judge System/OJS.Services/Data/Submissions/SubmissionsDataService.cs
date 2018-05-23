﻿namespace OJS.Services.Data.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsDataService : ISubmissionsDataService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;

        public SubmissionsDataService(IEfDeletableEntityRepository<Submission> submissions) =>
            this.submissions = submissions;

        public Submission GetBestForParticipantByProblem(int participantId, int problemId) =>
            this.GetAllByProblemAndParticipant(problemId, participantId)
                .Where(s => s.Processed)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault();

        public IQueryable<Submission> GetAll() => this.submissions.All();

        public IQueryable<Submission> GetByIdQuery(int id) =>
            this.submissions.All().Where(s => s.Id == id);

        public IQueryable<Submission> GetAllByProblem(int problemId) =>
            this.submissions.All().Where(s => s.ProblemId == problemId);

        public IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId) =>
            this.GetAllByProblem(problemId).Where(s => s.ParticipantId == participantId);

        public IQueryable<Submission> GetAllFromContestsByLecturer(string lecturerId) =>
            this.GetAll()
                .Where(s =>
                    (s.IsPublic.HasValue && s.IsPublic.Value) ||
                    s.Problem.ProblemGroup.Contest.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                    s.Problem.ProblemGroup.Contest.Category.Lecturers.Any(l => l.LecturerId == lecturerId));

        public IQueryable<Submission> GetAllCreatedBeforeDateAndNotBestCreatedBeforeDate(
            DateTime createdBeforeDate,
            DateTime notBestCreatedBeforeDate) =>
            this.submissions
                .AllWithDeleted()
                .Where(s => s.CreatedOn < createdBeforeDate ||
                    (s.CreatedOn < notBestCreatedBeforeDate &&
                        s.Participant.Scores.All(ps => ps.SubmissionId != s.Id)));

        public IEnumerable<int> GetIdsByProblem(int problemId) =>
            this.GetAllByProblem(problemId).Select(s => s.Id);

        public void SetAllToUnprocessedByProblem(int problemId) =>
            this.GetAllByProblem(problemId).Update(s => new Submission{ Processed = false });

        public void DeleteByProblem(int problemId) =>
            this.submissions.Delete(s => s.ProblemId == problemId);
    }
}